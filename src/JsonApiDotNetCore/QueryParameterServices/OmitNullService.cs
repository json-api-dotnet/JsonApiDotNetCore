using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class OmitNullService : QueryParameterService, IOmitNullService
    {
        private readonly IJsonApiOptions _options;

        public OmitNullService(IJsonApiOptions options)
        {
            Config = options.NullAttributeResponseBehavior.OmitNullValuedAttributes;
            _options = options;
        }

        /// <inheritdoc/>
        public bool Config { get; private set; }

        /// <inheritdoc/>
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
