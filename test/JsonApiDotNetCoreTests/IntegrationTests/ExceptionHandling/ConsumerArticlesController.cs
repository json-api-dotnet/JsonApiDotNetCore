#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    public sealed class ConsumerArticlesController : JsonApiController<ConsumerArticle, int>
    {
        public ConsumerArticlesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ConsumerArticle, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
