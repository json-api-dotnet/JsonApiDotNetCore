using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class ConsumerArticlesController : JsonApiController<ConsumerArticle>
    {
        public ConsumerArticlesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ConsumerArticle> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
