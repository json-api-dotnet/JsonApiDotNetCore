using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted
{
    public class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<Article> resourceService)
            
            
            
            
            
            
            
            : base(jsonApiOptions, jsonApiContext, resourceService)
        { }
    }
}
