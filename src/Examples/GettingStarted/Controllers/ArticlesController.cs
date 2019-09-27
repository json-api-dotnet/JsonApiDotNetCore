using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;

namespace GettingStarted
{
    public class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<Article> resourceService)
            : base(jsonApiOptions, resourceGraph, resourceService)
        { }
    }
}
