using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {
        //public override IEnumerable<Tag> AfterRead(IEnumerable<Tag> entities, ResourceAction pipeline, bool nestedHook = false)
        //{
        //    return entities.Where(t => t.Name != "This should be not be included");
        //}
    }
}
