using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    [ApiController]
    [DisableRoutingConvention]
    [Route("world-civilians")]
    partial class CiviliansController
    {
        [HttpGet("missing")]
        public async Task<IActionResult> GetMissingAsync()
        {
            await Task.Yield();
            return NotFound();
        }
    }
}
