using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.QueryStrings;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// This implementation of the behaviour provider reads the query params that
    /// can, if provided, override the settings in <see cref="IJsonApiOptions"/>.
    /// </summary>
    public sealed class ResourceObjectBuilderSettingsProvider : IResourceObjectBuilderSettingsProvider
    {
        private readonly IDefaultsQueryStringParameterReader _defaultsReader;
        private readonly INullsQueryStringParameterReader _nullsReader;

        public ResourceObjectBuilderSettingsProvider(IDefaultsQueryStringParameterReader defaultsReader, INullsQueryStringParameterReader nullsReader)
        {
            _defaultsReader = defaultsReader;
            _nullsReader = nullsReader;
        }

        /// <inheritdoc/>
        public ResourceObjectBuilderSettings Get()
        {
            return new ResourceObjectBuilderSettings(_nullsReader.SerializerNullValueHandling, _defaultsReader.SerializerDefaultValueHandling);
        }
    }
}
