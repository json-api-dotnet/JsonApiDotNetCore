using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableQuery(StandardQueryStringParameters.Sort | StandardQueryStringParameters.Page)]
    public sealed class ArticlesController : JsonApiController<Article>
    {
        public ArticlesController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Article> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
