using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public class BlogsHooksDefintion : ResourceHooksDefinition<Blog>
    {
        public BlogsHooksDefintion(IResourceGraph resourceGraph) : base(resourceGraph)
        {
        }

        public override IEnumerable<Blog> OnReturn(HashSet<Blog> resources, ResourcePipeline pipeline)
        {
            return resources.Where(t => t.Title != "This should not be included");
        }
    }
}
