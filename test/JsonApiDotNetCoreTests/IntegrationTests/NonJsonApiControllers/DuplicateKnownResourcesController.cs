using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

public sealed class DuplicateKnownResourcesController : JsonApiController<KnownResource, int>
{
    public DuplicateKnownResourcesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<KnownResource, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
