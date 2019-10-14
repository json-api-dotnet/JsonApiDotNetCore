using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Query
{
    public class OmitDefaultService : QueryParameterService, IOmitDefaultService
    {
        private readonly IOmitAttributeValueService _attributeBehaviourService;

        public OmitDefaultService(IOmitAttributeValueService attributeBehaviourService)
        {
            _attributeBehaviourService = attributeBehaviourService;
        }

        public override void Parse(string key, string value)
        {
            if (!bool.TryParse(value, out var config))
                throw new JsonApiException(400, $"{config} is not a valid option");
            _attributeBehaviourService.OmitDefaultValuedAttributes = config;
        }
    }
}
