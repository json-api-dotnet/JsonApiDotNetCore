using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    public sealed class ShipmentsController : JsonApiController<Shipment, int>
    {
        public ShipmentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Shipment> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
