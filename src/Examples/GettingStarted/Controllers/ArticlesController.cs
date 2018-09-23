using GettingStarted.Models;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    public class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(
          IJsonApiContext jsonApiContext,
          IResourceService<Article> resourceService,
          ILoggerFactory loggerFactory)
          : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}