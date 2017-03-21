using System.Collections.Generic;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCoreExampleTests.Services
{
    public class MetaService : IRequestMeta
    {
        public Dictionary<string, object> GetMeta()
        {
            return new Dictionary<string, object> {
                { "request-meta", "request-meta-value" }
            };
        }
    }
}
