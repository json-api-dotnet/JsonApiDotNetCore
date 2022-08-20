using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed partial class SofasController : JsonApiController<Sofa, int>
{
    public SofasController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Sofa, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
