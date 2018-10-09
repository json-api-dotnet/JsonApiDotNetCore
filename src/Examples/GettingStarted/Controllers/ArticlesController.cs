using GettingStarted.Models;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted
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