using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    public sealed class ThrowingArticlesController : JsonApiController<ThrowingArticle, int>
    {
        public ThrowingArticlesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ThrowingArticle> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
