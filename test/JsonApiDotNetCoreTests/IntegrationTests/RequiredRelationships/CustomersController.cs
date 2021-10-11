#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    public sealed class CustomersController : JsonApiController<Customer, int>
    {
        public CustomersController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Customer, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
