using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

public sealed partial class WebAccountsController : JsonApiController<WebAccount, int>
{
    public WebAccountsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<WebAccount, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
