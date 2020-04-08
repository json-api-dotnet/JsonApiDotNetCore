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
        private readonly IOmitDefaultService _defaultAttributeValues;
        private readonly IOmitNullService _nullAttributeValues;

        public ResourceObjectBuilderSettingsProvider(IOmitDefaultService defaultAttributeValues,
                                                     IOmitNullService nullAttributeValues)
        {
            _defaultAttributeValues = defaultAttributeValues;
            _nullAttributeValues = nullAttributeValues;
        }

        /// <inheritdoc/>
        public ResourceObjectBuilderSettings Get()
        {
            return new ResourceObjectBuilderSettings(_nullAttributeValues.OmitAttributeIfValueIsNull, _defaultAttributeValues.OmitAttributeIfValueIsDefault);
        }
    }
}
