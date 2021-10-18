#nullable disable

using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    [ApiController]
    [DisableRoutingConvention]
    [Route("world-civilians")]
    public sealed class CiviliansController : JsonApiController<Civilian, int>
    {
        public CiviliansController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Civilian, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }

        [HttpGet("missing")]
        public async Task<IActionResult> GetMissingAsync()
        {
            await Task.Yield();
            return NotFound();
        }
    }
}
