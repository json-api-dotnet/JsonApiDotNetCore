using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class CylindersController : JsonApiController<Cylinder, long>
{
    public CylindersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Cylinder, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
