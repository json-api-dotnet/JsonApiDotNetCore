using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class ShipmentsController : JsonApiController<Shipment>
    {
        public ShipmentsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Shipment> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
