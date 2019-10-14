using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Query
{
    public class OmitNullService : QueryParameterService, IOmitNullService
    {
        private readonly IJsonApiOptions _options;

        public OmitNullService(IJsonApiOptions options)
        {
            Config = options.NullAttributeResponseBehavior.OmitNullValuedAttributes;
            _options = options;
        }

        public bool Config { get; private set; }

        public override void Parse(string key, string value)
        {
            if (!_options.NullAttributeResponseBehavior.AllowClientOverride)
                return;

            if (!bool.TryParse(value, out var config))
                return;

            Config = config;
        }
    }
}
