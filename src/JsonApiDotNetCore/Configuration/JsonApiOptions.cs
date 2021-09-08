using System.Data;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class JsonApiOptions : IJsonApiOptions
    {
        // Workaround for https://github.com/dotnet/efcore/issues/21026
        internal bool DisableTopPagination { get; set; }
        internal bool DisableChildrenPagination { get; set; }

        /// <inheritdoc />
        public string Namespace { get; set; }

        /// <inheritdoc />
        public AttrCapabilities DefaultAttrCapabilities { get; set; } = AttrCapabilities.All;

        /// <inheritdoc />
        public bool IncludeJsonApiVersion { get; set; }

        /// <inheritdoc />
        public bool IncludeExceptionStackTraceInErrors { get; set; }

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
        public PageSize DefaultPageSize { get; set; } = new(10);

        /// <inheritdoc />
        public PageSize MaximumPageSize { get; set; }

        /// <inheritdoc />
        public PageNumber MaximumPageNumber { get; set; }

        /// <inheritdoc />
        public bool ValidateModelState { get; set; }

        /// <inheritdoc />
        public bool AllowClientGeneratedIds { get; set; }

        /// <inheritdoc />
        public bool AllowUnknownQueryStringParameters { get; set; }

        /// <inheritdoc />
        public bool EnableLegacyFilterNotation { get; set; }

        /// <inheritdoc />
        public int? MaximumIncludeDepth { get; set; }

        /// <inheritdoc />
        public int? MaximumOperationsPerRequest { get; set; } = 10;

        /// <inheritdoc />
        public IsolationLevel? TransactionIsolationLevel { get; set; }

        public JsonSerializerSettings SerializerSettings { get; } = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        /// <inheritdoc />
        public JsonSerializerOptions SerializerOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
