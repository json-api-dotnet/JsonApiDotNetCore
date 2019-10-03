using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.QueryServices.Contracts;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    /// <summary>
    /// This implementation of the behaviour provider reads the query params that
    /// can, if provided, override the settings in <see cref="IJsonApiOptions"/>.
    /// </summary>
    public class ServerSerializerSettingsProvider : ISerializerSettingsProvider
    {
        private readonly IJsonApiOptions _options;
        private readonly IAttributeBehaviourQueryService _attributeBehaviour;

        public ServerSerializerSettingsProvider(IJsonApiOptions options, IAttributeBehaviourQueryService attributeBehaviour)
        {
            _options = options;
            _attributeBehaviour = attributeBehaviour;
        }

        /// <inheritdoc/>
        public SerializerSettings Get()
        {
            bool omitNullConfig;
            if (_attributeBehaviour.OmitNullValuedAttributes.HasValue)
                omitNullConfig = _attributeBehaviour.OmitNullValuedAttributes.Value;
            else omitNullConfig = _options.NullAttributeResponseBehavior.OmitNullValuedAttributes;

            bool omitDefaultConfig;
            if (_attributeBehaviour.OmitDefaultValuedAttributes.HasValue)
                omitDefaultConfig = _attributeBehaviour.OmitDefaultValuedAttributes.Value;
            else omitDefaultConfig = _options.DefaultAttributeResponseBehavior.OmitDefaultValuedAttributes;

            return new SerializerSettings(omitNullConfig, omitDefaultConfig);
        }
    }
}
