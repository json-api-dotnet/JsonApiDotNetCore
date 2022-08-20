using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed partial class ChromeWheelsController : JsonApiController<ChromeWheel, long>
{
    public ChromeWheelsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ChromeWheel, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
