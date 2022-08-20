using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

public sealed partial class UnknownResourcesController : JsonApiController<UnknownResource, int>
{
    public UnknownResourcesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<UnknownResource, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
