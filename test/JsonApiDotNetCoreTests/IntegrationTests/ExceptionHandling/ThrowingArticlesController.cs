using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class ThrowingArticlesController : JsonApiController<ThrowingArticle>
    {
        public ThrowingArticlesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ThrowingArticle> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
