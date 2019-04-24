using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {
        public TagResource(IHooksDiscovery<Tag> hooks = null) : base(hooks) { }

        public override void BeforeRead(ResourceAction actionSource, string stringId = null)
        {
            return;
        }

        public override IEnumerable<Tag> AfterRead(IEnumerable<Tag> entities, ResourceAction actionSource)
        {
            return entities.Where(t => t.Name != "This should be not be included").ToList();
        }
    }
}
