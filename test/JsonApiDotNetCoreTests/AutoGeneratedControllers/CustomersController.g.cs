using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships;

public sealed partial class CustomersController : JsonApiController<Customer, int>
{
    public CustomersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Customer, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
