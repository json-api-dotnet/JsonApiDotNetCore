using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Query
{
    public class OmitDefaultService : QueryParameterService, IOmitDefaultService
    {
        private readonly IJsonApiOptions _options;

        public OmitDefaultService(IJsonApiOptions options)
        {
            Config = options.DefaultAttributeResponseBehavior.OmitDefaultValuedAttributes;
            _options = options;
        }

        public bool Config { get; private set; }

        public override void Parse(string key, string value)
        {
            if (!_options.DefaultAttributeResponseBehavior.AllowClientOverride)
                return;

            if (!bool.TryParse(value, out var config))
                return;

            Config = config;
        }
    }
}
