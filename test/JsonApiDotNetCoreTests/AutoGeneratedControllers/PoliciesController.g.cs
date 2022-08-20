using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

public sealed partial class PoliciesController : JsonApiController<Policy, int>
{
    public PoliciesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Policy, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
