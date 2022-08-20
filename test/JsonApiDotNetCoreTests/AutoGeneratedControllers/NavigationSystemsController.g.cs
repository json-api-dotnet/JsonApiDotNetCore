using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class NavigationSystemsController : JsonApiController<NavigationSystem, long>
{
    public NavigationSystemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<NavigationSystem, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
