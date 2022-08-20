using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

public sealed partial class HumansController : JsonApiController<Human, int>
{
    public HumansController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Human, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
