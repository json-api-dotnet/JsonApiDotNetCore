using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {
        public override IEnumerable<Tag> AfterRead(IEnumerable<Tag> entities, ResourceAction actionSource)
        {
            return entities.Where(t => t.Name != "THISTAGSHOULDNOTBEVISIBLE").ToList();
        }
    }
}
