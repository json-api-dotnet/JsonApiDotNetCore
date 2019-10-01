using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.QueryServices.Contracts;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    public class ServerSerializerBehaviourProvider : ISerializerBehaviourProvider
    {
        private readonly IJsonApiOptions _options;
        private readonly IAttributeBehaviourQuery _attributeBehaviour;

        public ServerSerializerBehaviourProvider(IJsonApiOptions options, IAttributeBehaviourQuery attributeBehaviour)
        {
            _options = options;
            _attributeBehaviour = attributeBehaviour;
        }

        public SerializerBehaviour GetBehaviour()
        {
            bool omitNullConfig;
            if (_attributeBehaviour.OmitNullValuedAttributes.HasValue)
                omitNullConfig = _attributeBehaviour.OmitNullValuedAttributes.Value;
            else omitNullConfig = _options.NullAttributeResponseBehavior.OmitNullValuedAttributes;

            bool omitDefaultConfig;
            if (_attributeBehaviour.OmitDefaultValuedAttributes.HasValue)
                omitDefaultConfig = _attributeBehaviour.OmitDefaultValuedAttributes.Value;
            else omitDefaultConfig = _options.DefaultAttributeResponseBehavior.OmitDefaultValuedAttributes;

            return new SerializerBehaviour(omitNullConfig, omitDefaultConfig);
        }
    }
}
