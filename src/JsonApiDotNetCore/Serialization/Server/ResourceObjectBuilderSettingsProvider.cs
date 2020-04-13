using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// This implementation of the behaviour provider reads the query params that
    /// can, if provided, override the settings in <see cref="IJsonApiOptions"/>.
    /// </summary>
    public sealed class ResourceObjectBuilderSettingsProvider : IResourceObjectBuilderSettingsProvider
    {
        private readonly IDefaultsService _defaultsService;
        private readonly INullsService _nullsService;

        public ResourceObjectBuilderSettingsProvider(IDefaultsService defaultsService, INullsService nullsService)
        {
            _defaultsService = defaultsService;
            _nullsService = nullsService;
        }

        /// <inheritdoc/>
        public ResourceObjectBuilderSettings Get()
        {
            return new ResourceObjectBuilderSettings(_nullsService.SerializerNullValueHandling, _defaultsService.SerializerDefaultValueHandling);
        }
    }
}
