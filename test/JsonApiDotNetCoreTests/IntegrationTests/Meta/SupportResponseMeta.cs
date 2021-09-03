using System.Collections.Generic;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class SupportResponseMeta : IResponseMeta
    {
        public IReadOnlyDictionary<string, object> GetMeta()
        {
            return new Dictionary<string, object>
            {
                ["license"] = "MIT",
                ["projectUrl"] = "https://github.com/json-api-dotnet/JsonApiDotNetCore/",
                ["versions"] = new[]
                {
                    "v4.0.0",
                    "v3.1.0",
                    "v2.5.2",
                    "v1.3.1"
                }
            };
        }
    }
}
