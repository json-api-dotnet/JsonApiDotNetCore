using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {
        public override void BeforeRead(HookExecutionContext<Tag> context, string stringId = null)
        {
            return;
        }

        public override IEnumerable<Tag> AfterRead(IEnumerable<Tag> entities, HookExecutionContext<Tag> context)
        {
            return entities.Where(t => t.Name != "This should be not be included");
        }


    }
}
