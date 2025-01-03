using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Configuration;

/// <inheritdoc cref="IJsonApiOptions" />
[PublicAPI]
public sealed class JsonApiOptions : IJsonApiOptions
{
    private static readonly IReadOnlySet<JsonApiMediaTypeExtension> EmptyExtensionSet = new HashSet<JsonApiMediaTypeExtension>().AsReadOnly();
    private readonly Lazy<JsonSerializerOptions> _lazySerializerWriteOptions;
    private readonly Lazy<JsonSerializerOptions> _lazySerializerReadOptions;

    /// <inheritdoc />
    JsonSerializerOptions IJsonApiOptions.SerializerReadOptions => _lazySerializerReadOptions.Value;

    /// <inheritdoc />
    JsonSerializerOptions IJsonApiOptions.SerializerWriteOptions => _lazySerializerWriteOptions.Value;

    /// <inheritdoc />
    public string? Namespace { get; set; }

    /// <inheritdoc />
    public AttrCapabilities DefaultAttrCapabilities { get; set; } = AttrCapabilities.All;

    /// <inheritdoc />
    public HasOneCapabilities DefaultHasOneCapabilities { get; set; } = HasOneCapabilities.All;

    /// <inheritdoc />
    public HasManyCapabilities DefaultHasManyCapabilities { get; set; } = HasManyCapabilities.All;

    /// <inheritdoc />
    public bool IncludeJsonApiVersion { get; set; }

    /// <inheritdoc />
    public bool IncludeExceptionStackTraceInErrors { get; set; }

    /// <inheritdoc />
    public bool IncludeRequestBodyInErrors { get; set; }

    /// <inheritdoc />
    public bool UseRelativeLinks { get; set; }

    /// <inheritdoc />
    public LinkTypes TopLevelLinks { get; set; } = LinkTypes.All;

    /// <inheritdoc />
    public LinkTypes ResourceLinks { get; set; } = LinkTypes.All;

    /// <inheritdoc />
    public LinkTypes RelationshipLinks { get; set; } = LinkTypes.All;

    /// <inheritdoc />
    public bool IncludeTotalResourceCount { get; set; }

    /// <inheritdoc />
    public PageSize? DefaultPageSize { get; set; } = new(10);

    /// <inheritdoc />
    public PageSize? MaximumPageSize { get; set; }

    /// <inheritdoc />
    public PageNumber? MaximumPageNumber { get; set; }

    /// <inheritdoc />
    public bool ValidateModelState { get; set; } = true;

    /// <inheritdoc />
    public ClientIdGenerationMode ClientIdGeneration { get; set; }

    /// <inheritdoc />
    [Obsolete("Use ClientIdGeneration instead.")]
    public bool AllowClientGeneratedIds
    {
        get => ClientIdGeneration is ClientIdGenerationMode.Allowed or ClientIdGenerationMode.Required;
        set => ClientIdGeneration = value ? ClientIdGenerationMode.Allowed : ClientIdGenerationMode.Forbidden;
    }

    /// <inheritdoc />
    public bool AllowUnknownQueryStringParameters { get; set; }

    /// <inheritdoc />
    public bool AllowUnknownFieldsInRequestBody { get; set; }

    /// <inheritdoc />
    public bool EnableLegacyFilterNotation { get; set; }

    /// <inheritdoc />
    public int? MaximumIncludeDepth { get; set; }

    /// <inheritdoc />
    public int? MaximumOperationsPerRequest { get; set; } = 10;

    /// <inheritdoc />
    public IsolationLevel? TransactionIsolationLevel { get; set; }

    /// <inheritdoc />
    public IReadOnlySet<JsonApiMediaTypeExtension> Extensions { get; set; } = EmptyExtensionSet;

    /// <inheritdoc />
    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        // These are the options common to serialization and deserialization.
        // At runtime, we actually use SerializerReadOptions and SerializerWriteOptions, which are customized copies of these settings,
        // to overcome the limitation in System.Text.Json that the JsonPath is incorrect when using custom converters.
        // Therefore we try to avoid using custom converters has much as possible.
        // https://github.com/Tarmil/FSharp.SystemTextJson/issues/37
        // https://github.com/dotnet/runtime/issues/50205#issuecomment-808401245

        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new SingleOrManyDataConverterFactory()
        }
    };

    public JsonApiOptions()
    {
        _lazySerializerReadOptions =
            new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions(SerializerOptions), LazyThreadSafetyMode.ExecutionAndPublication);

        _lazySerializerWriteOptions = new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions(SerializerOptions)
        {
            Converters =
            {
                new WriteOnlyDocumentConverter(),
                new WriteOnlyRelationshipObjectConverter()
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Adds the specified JSON:API extensions to the existing <see cref="Extensions" /> set.
    /// </summary>
    /// <param name="extensionsToAdd">
    /// The JSON:API extensions to add.
    /// </param>
    public void IncludeExtensions(params JsonApiMediaTypeExtension[] extensionsToAdd)
    {
        ArgumentNullException.ThrowIfNull(extensionsToAdd);

        if (!Extensions.IsSupersetOf(extensionsToAdd))
        {
            var extensions = new HashSet<JsonApiMediaTypeExtension>(Extensions);

            foreach (JsonApiMediaTypeExtension extension in extensionsToAdd)
            {
                extensions.Add(extension);
            }

            Extensions = extensions.AsReadOnly();
        }
    }
}
