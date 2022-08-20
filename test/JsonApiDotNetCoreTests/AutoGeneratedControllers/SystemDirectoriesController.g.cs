using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

public sealed partial class SystemDirectoriesController : JsonApiController<SystemDirectory, int>
{
    public SystemDirectoriesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<SystemDirectory, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
