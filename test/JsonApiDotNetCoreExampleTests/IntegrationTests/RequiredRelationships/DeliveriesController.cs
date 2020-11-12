using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class DeliveriesController : JsonApiController<Delivery>
    {
        public DeliveriesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Delivery> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
