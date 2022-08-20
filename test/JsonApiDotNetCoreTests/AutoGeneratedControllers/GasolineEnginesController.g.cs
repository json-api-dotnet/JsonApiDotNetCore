using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class GasolineEnginesController : JsonApiController<GasolineEngine, long>
{
    public GasolineEnginesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<GasolineEngine, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
