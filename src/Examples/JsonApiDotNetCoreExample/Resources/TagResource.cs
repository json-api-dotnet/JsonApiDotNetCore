using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {
        public override IEnumerable<Tag> OnReturn(IEnumerable<Tag> entities, ResourceAction pipeline)
        {
            return entities.Where(t => t.Name != "This should be not be included");
        }
    }
}
