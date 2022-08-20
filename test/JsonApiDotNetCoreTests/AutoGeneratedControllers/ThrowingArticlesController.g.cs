using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

public sealed partial class ThrowingArticlesController : JsonApiController<ThrowingArticle, int>
{
    public ThrowingArticlesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ThrowingArticle, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
