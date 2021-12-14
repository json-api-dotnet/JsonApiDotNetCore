using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

// Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
public partial class CiviliansController
{
}

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
