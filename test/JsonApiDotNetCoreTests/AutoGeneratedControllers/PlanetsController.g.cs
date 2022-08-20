using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed partial class PlanetsController : JsonApiController<Planet, int>
{
    public PlanetsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Planet, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
