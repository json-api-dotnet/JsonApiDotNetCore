using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.QueryStrings;

namespace JsonApiDotNetCore.Serialization.Building
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
            _defaultsReader = defaultsReader ?? throw new ArgumentNullException(nameof(defaultsReader));
            _nullsReader = nullsReader ?? throw new ArgumentNullException(nameof(nullsReader));
        }

        /// <inheritdoc/>
        public ResourceObjectBuilderSettings Get()
        {
            return new ResourceObjectBuilderSettings(_nullsReader.SerializerNullValueHandling, _defaultsReader.SerializerDefaultValueHandling);
        }
    }
}
