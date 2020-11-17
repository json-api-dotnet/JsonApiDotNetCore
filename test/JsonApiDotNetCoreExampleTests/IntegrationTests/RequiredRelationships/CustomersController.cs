using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class CustomersController : JsonApiController<Customer>
    {
        public CustomersController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Customer> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
