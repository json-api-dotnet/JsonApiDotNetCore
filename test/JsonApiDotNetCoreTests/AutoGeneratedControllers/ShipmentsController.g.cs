using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

public sealed partial class ShipmentsController : JsonApiController<Shipment, int>
{
    public ShipmentsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Shipment, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
