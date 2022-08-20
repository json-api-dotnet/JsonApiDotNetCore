using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

public sealed partial class PerformersController : JsonApiController<Performer, int>
{
    public PerformersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Performer, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
