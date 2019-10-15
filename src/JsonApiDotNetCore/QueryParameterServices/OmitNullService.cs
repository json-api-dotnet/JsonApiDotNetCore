using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using Microsoft.Extensions.Primitives;

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

        public virtual void Parse(KeyValuePair<string, StringValues> queryParameter)
        {
            if (!_options.NullAttributeResponseBehavior.AllowClientOverride)
                return;

            if (!bool.TryParse(queryParameter.Value, out var config))
                return;

            Config = config;
        }
    }
}
