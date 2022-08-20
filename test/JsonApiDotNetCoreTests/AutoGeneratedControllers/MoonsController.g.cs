using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed partial class MoonsController : JsonApiController<Moon, int>
{
    public MoonsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Moon, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
