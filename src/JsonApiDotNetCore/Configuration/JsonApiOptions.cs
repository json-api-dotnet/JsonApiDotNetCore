using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class JsonApiOptions : IJsonApiOptions
    {
        private Lazy<JsonSerializerOptions> _lazySerializerWriteOptions;
        private Lazy<JsonSerializerOptions> _lazySerializerReadOptions;

        /// <inheritdoc />
        JsonSerializerOptions IJsonApiOptions.SerializerReadOptions => _lazySerializerReadOptions.Value;

        /// <inheritdoc />
        JsonSerializerOptions IJsonApiOptions.SerializerWriteOptions => _lazySerializerWriteOptions.Value;

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

        public JsonApiOptions()
        {
            _lazySerializerReadOptions =
                new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions(SerializerOptions), LazyThreadSafetyMode.PublicationOnly);

            _lazySerializerWriteOptions = new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions(SerializerOptions)
            {
                Converters =
                {
                    new WriteOnlyDocumentConverter(),
                    new WriteOnlyRelationshipObjectConverter()
                }
            }, LazyThreadSafetyMode.PublicationOnly);
        }
    }
}
