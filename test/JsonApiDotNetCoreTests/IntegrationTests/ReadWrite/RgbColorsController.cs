using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    public sealed class RgbColorsController : JsonApiController<RgbColor, string>
    {
        public RgbColorsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<RgbColor, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
