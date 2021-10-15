#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    public sealed class RgbColorsController : JsonApiController<RgbColor, string>
    {
        public RgbColorsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<RgbColor, string> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
