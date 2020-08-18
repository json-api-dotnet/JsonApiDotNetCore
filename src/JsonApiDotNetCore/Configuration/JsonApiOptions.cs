using JsonApiDotNetCore.Resources.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Configuration
{
    /// <inheritdoc/>
    public sealed class JsonApiOptions : IJsonApiOptions
    {
        /// <inheritdoc/>
        public string Namespace { get; set; }

        /// <inheritdoc/>
        public AttrCapabilities DefaultAttrCapabilities { get; set; } = AttrCapabilities.All;

        /// <inheritdoc/>
        public bool IncludeExceptionStackTraceInErrors { get; set; }

        /// <inheritdoc/>
        public bool UseRelativeLinks { get; set; }

        /// <inheritdoc/>
        public Links TopLevelLinks { get; set; } = Links.All;

        /// <inheritdoc/>
        public Links ResourceLinks { get; set; } = Links.All;

        /// <inheritdoc/>
        public Links RelationshipLinks { get; set; } = Links.All;

        /// <inheritdoc/>
        public bool IncludeTotalResourceCount { get; set; }

        /// <inheritdoc/>
        public PageSize DefaultPageSize { get; set; } = new PageSize(10);

        /// <inheritdoc/>
        public PageSize MaximumPageSize { get; set; }

        /// <inheritdoc/>
        public PageNumber MaximumPageNumber { get; set; }

        /// <inheritdoc/>
        public bool ValidateModelState { get; set; }

        /// <inheritdoc/>
        public bool AllowClientGeneratedIds { get; set; }

        /// <inheritdoc/>
        public bool EnableResourceHooks { get; set; }

        /// <inheritdoc/>
        public bool LoadDatabaseValues { get; set; }

        /// <inheritdoc/>
        public bool AllowUnknownQueryStringParameters { get; set; }

        /// <inheritdoc/>
        public bool EnableLegacyFilterNotation { get; set; }

        /// <inheritdoc/>
        public bool AllowQueryStringOverrideForSerializerNullValueHandling { get; set; }

        /// <inheritdoc/>
        public bool AllowQueryStringOverrideForSerializerDefaultValueHandling { get; set; }

        public int? MaximumIncludeDepth { get; set; }

        /// <inheritdoc/>
        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        /// <summary>
        /// Provides an interface for formatting relationship id properties given the navigation property name
        /// </summary>
        public static IRelatedIdMapper RelatedIdMapper { get; set; } = new RelatedIdMapper();

        // Workaround for https://github.com/dotnet/efcore/issues/21026
        internal bool DisableTopPagination { get; set; }
        internal bool DisableChildrenPagination { get; set; }
    }
}
