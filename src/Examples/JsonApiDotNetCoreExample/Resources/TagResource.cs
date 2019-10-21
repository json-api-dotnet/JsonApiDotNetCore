using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {
        public TagResource(IContextEntityProvider provider) : base(provider) { }

        public override IEnumerable<Tag> BeforeCreate(IEntityHashSet<Tag> affected, ResourcePipeline pipeline)
        {
            return base.BeforeCreate(affected, pipeline);
        }

        public override IEnumerable<Tag> OnReturn(HashSet<Tag> entities, ResourcePipeline pipeline)
        {
            return entities.Where(t => t.Name != "This should be not be included");
        }
    }
}
