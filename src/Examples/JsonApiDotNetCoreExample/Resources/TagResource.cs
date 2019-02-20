using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {

        public override IQueryable<Tag> OnList(IQueryable<Tag> entities) => entities.Where(t => t.Name != "THISTAGSHOULDNOTBEVISIBLE");
    }
}
