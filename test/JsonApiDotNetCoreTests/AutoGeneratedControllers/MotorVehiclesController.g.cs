using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class MotorVehiclesController : JsonApiController<MotorVehicle, long>
{
    public MotorVehiclesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<MotorVehicle, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
