using System.Collections.Generic;
using System.Linq;
using System;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class ArticleResource : ResourceDefinition<Article>
    {

        public override IEnumerable<Article> AfterRead(IEnumerable<Article> entities, ResourceAction actionSource)
        {
            if (actionSource == ResourceAction.GetSingle && entities.Single().Name == "Classified")
            {
                throw new JsonApiException(401, "Not Allowed", new UnauthorizedAccessException());
            }
            return entities.Where(t => t.Name != "This should be not be included");
        }
    }

}
