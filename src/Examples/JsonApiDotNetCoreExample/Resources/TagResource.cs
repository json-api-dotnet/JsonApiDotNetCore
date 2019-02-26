using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TagResource : ResourceDefinition<Tag>
    {

        public override List<Tag> OnList(List<Tag> entities)
        {
            return entities.Where(t => t.Name != "THISTAGSHOULDNOTBEVISIBLE").ToList();
        }

    }
}
