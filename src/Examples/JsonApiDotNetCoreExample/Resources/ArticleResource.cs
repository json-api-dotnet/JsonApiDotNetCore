using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCoreExample.Resources
{
    public class ArticleResource : ResourceDefinition<Article>
    {
        public ArticleResource(IResourceGraph resourceGraph) : base(resourceGraph) { }

        public override IEnumerable<Article> OnReturn(HashSet<Article> entities, ResourcePipeline pipeline)
        {
            if (pipeline == ResourcePipeline.GetSingle && entities.Single().Name == "Classified")
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "You are not allowed to see this article."
                });
            }

            return entities.Where(t => t.Name != "This should not be included");
        }
    }
}

