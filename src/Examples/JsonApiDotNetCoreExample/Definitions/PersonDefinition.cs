using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public class PersonDefinition : JsonApiResourceDefinition<Person>
    {
        public PersonDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
        }

        public override IReadOnlyDictionary<string, object> GetMeta()
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
