using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    [DisableRoutingConvention]
    [Route("custom/path/to/paintings")]
    public sealed class PaintingsController : JsonApiController<Painting>
    {
        public PaintingsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Painting> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
