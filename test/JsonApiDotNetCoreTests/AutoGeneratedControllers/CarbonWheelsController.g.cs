using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class CarbonWheelsController : JsonApiController<CarbonWheel, long>
{
    public CarbonWheelsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<CarbonWheel, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
