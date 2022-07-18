using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Configuration;

/// <inheritdoc />
[PublicAPI]
public sealed class JsonApiOptions : IJsonApiOptions
{
    private readonly Lazy<JsonApiSerializationContext> _lazySerializerReadContext;
    private readonly Lazy<JsonApiSerializationContext> _lazySerializerWriteContext;

    /// <inheritdoc />
    JsonApiSerializationContext IJsonApiOptions.SerializationReadContext => _lazySerializerReadContext.Value;

    /// <inheritdoc />
    JsonSerializerOptions IJsonApiOptions.SerializerReadOptions => ((IJsonApiOptions)this).SerializationReadContext.Options;

    /// <inheritdoc />
    JsonApiSerializationContext IJsonApiOptions.SerializationWriteContext => _lazySerializerWriteContext.Value;

    /// <inheritdoc />
    JsonSerializerOptions IJsonApiOptions.SerializerWriteOptions => ((IJsonApiOptions)this).SerializationWriteContext.Options;

    /// <inheritdoc />
    public string? Namespace { get; set; }

    /// <inheritdoc />
    public AttrCapabilities DefaultAttrCapabilities { get; set; } = AttrCapabilities.All;

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
    public bool AllowClientGeneratedIds { get; set; }

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

    static JsonApiOptions()
    {
        // Bug workaround for https://github.com/dotnet/efcore/issues/27436
        AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue26779", true);
    }

    public JsonApiOptions()
    {
        _lazySerializerReadContext = new Lazy<JsonApiSerializationContext>(() => new JsonApiSerializationContext(new JsonSerializerOptions(SerializerOptions)),
            LazyThreadSafetyMode.ExecutionAndPublication);

        _lazySerializerWriteContext = new Lazy<JsonApiSerializationContext>(() => new JsonApiSerializationContext(new JsonSerializerOptions(SerializerOptions)
        {
            Converters =
            {
                new WriteOnlyDocumentConverter(),
                new WriteOnlyRelationshipObjectConverter()
            }
        }), LazyThreadSafetyMode.ExecutionAndPublication);
    }
}
