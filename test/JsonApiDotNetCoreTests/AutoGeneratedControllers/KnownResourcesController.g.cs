using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

public sealed partial class KnownResourcesController : JsonApiController<KnownResource, int>
{
    public KnownResourcesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<KnownResource, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
