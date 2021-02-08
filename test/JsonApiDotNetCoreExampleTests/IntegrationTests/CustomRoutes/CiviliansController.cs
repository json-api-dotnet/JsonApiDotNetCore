using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    [ApiController]
    [DisableRoutingConvention, Route("world-civilians")]
    public sealed class CiviliansController : JsonApiController<Civilian>
    {
        public CiviliansController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Civilian> resourceService)
            : base(options, loggerFactory, resourceService)
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
