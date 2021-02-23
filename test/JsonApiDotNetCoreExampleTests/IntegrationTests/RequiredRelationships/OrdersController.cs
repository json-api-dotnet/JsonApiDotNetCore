using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class OrdersController : JsonApiController<Order>
    {
        public OrdersController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Order> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
