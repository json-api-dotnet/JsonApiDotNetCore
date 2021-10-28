using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    public sealed class MoonsController : JsonApiController<Moon, int>
    {
        public MoonsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Moon, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
