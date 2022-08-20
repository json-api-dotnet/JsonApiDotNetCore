using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

public sealed partial class WebShopsController : JsonApiController<WebShop, int>
{
    public WebShopsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<WebShop, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
