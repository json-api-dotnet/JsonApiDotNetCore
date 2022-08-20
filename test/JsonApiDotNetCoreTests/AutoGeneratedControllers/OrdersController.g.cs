using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

public sealed partial class OrdersController : JsonApiController<Order, int>
{
    public OrdersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Order, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
