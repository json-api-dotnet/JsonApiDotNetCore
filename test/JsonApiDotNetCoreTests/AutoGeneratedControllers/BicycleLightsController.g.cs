using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class BicycleLightsController : JsonApiController<BicycleLight, long>
{
    public BicycleLightsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<BicycleLight, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
