using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

public sealed partial class ConsumerArticlesController : JsonApiController<ConsumerArticle, int>
{
    public ConsumerArticlesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ConsumerArticle, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
