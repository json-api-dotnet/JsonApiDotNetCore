using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

public sealed partial class MenController : JsonApiController<Man, int>
{
    public MenController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Man, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
