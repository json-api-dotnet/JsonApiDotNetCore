using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.ApiControllerAnnotation;

[ApiController]
[Route("[controller]")]
partial class LoginTokensController
{
    [HttpGet("missing")]
    public async Task<IActionResult> GetMissingAsync()
    {
        await Task.Yield();
        return NotFound();
    }
}
