using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

public sealed partial class DealershipsController : JsonApiController<Dealership, int>
{
    public DealershipsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Dealership, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
