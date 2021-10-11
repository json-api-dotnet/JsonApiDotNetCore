#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS
{
    [DisableRoutingConvention]
    [Route("custom/path/to/paintings-of-the-world")]
    public sealed class PaintingsController : JsonApiController<Painting, int>
    {
        public PaintingsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Painting, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
