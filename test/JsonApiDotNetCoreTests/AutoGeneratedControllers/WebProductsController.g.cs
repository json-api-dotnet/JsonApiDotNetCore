using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

namespace JsonApiDotNetCoreTests.IntegrationTests.MultiTenancy;

public sealed partial class WebProductsController : JsonApiController<WebProduct, int>
{
    public WebProductsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<WebProduct, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
