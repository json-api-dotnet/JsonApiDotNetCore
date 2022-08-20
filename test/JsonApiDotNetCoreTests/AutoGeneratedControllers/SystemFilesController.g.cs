using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

public sealed partial class SystemFilesController : JsonApiController<SystemFile, int>
{
    public SystemFilesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<SystemFile, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
