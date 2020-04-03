using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCoreExample.Resources
{
    public class ArticleResource : ResourceDefinition<Article>
    {
        public ArticleResource(IResourceGraph resourceGraph) : base(resourceGraph) { }

        public override IEnumerable<Article> OnReturn(HashSet<Article> entities, ResourcePipeline pipeline)
        {
            if (pipeline == ResourcePipeline.GetSingle && entities.Single().Name == "Classified")
            {
                throw new JsonApiException(HttpStatusCode.Forbidden, "You are not allowed to see this article!");
            }
            return entities.Where(t => t.Name != "This should be not be included");
        }
    }
}

