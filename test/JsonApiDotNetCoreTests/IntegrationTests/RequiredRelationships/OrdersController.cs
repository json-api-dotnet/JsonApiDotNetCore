using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    public sealed class OrdersController : JsonApiController<Order, int>
    {
        public OrdersController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Order, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
