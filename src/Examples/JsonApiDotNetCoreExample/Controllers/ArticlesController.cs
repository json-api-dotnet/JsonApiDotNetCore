using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Article> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
