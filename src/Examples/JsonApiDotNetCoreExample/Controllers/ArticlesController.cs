using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(
            IJsonApiContext jsonApiContext,
            IResourceService<Article> resourceService) 
            : base(jsonApiContext, resourceService)
        { }
    }
}