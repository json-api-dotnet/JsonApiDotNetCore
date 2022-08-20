using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

public sealed partial class RgbColorsController : JsonApiController<RgbColor, string>
{
    public RgbColorsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<RgbColor, string> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
