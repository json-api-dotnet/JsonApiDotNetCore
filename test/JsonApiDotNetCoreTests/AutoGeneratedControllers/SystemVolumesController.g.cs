using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

public sealed partial class SystemVolumesController : JsonApiController<SystemVolume, int>
{
    public SystemVolumesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<SystemVolume, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
