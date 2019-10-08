using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <summary>
    /// This implementation of the behaviour provider reads the query params that
    /// can, if provided, override the settings in <see cref="IJsonApiOptions"/>.
    /// </summary>
    public class ResponseSerializerSettingsProvider : IResourceObjectBuilderSettingsProvider
    {
        private readonly IJsonApiOptions _options;
        private readonly IAttributeBehaviourService _attributeBehaviour;

        public ResponseSerializerSettingsProvider(IJsonApiOptions options, IAttributeBehaviourService attributeBehaviour)
        {
            _options = options;
            _attributeBehaviour = attributeBehaviour;
        }

        /// <inheritdoc/>
        public ResourceObjectBuilderSettings Get()
        {
            bool omitNullConfig;
            if (_attributeBehaviour.OmitNullValuedAttributes.HasValue)
                omitNullConfig = _attributeBehaviour.OmitNullValuedAttributes.Value;
            else omitNullConfig = _options.NullAttributeResponseBehavior.OmitNullValuedAttributes;

            bool omitDefaultConfig;
            if (_attributeBehaviour.OmitDefaultValuedAttributes.HasValue)
                omitDefaultConfig = _attributeBehaviour.OmitDefaultValuedAttributes.Value;
            else omitDefaultConfig = _options.DefaultAttributeResponseBehavior.OmitDefaultValuedAttributes;

            return new ResourceObjectBuilderSettings(omitNullConfig, omitDefaultConfig);
        }
    }
}
