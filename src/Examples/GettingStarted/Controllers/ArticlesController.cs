using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    public class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Article> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
