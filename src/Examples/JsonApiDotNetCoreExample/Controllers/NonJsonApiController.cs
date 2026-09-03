using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers;

[Route("[controller]")]
public sealed class NonJsonApiController : ControllerBase
{
    [HttpGet]
    [HttpHead]
    public IActionResult Get()
    {
        string[] result = ["Welcome!"];

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] string? name)
    {
        await Task.Yield();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Please send your name.");
        }

        string result = $"Hello, {name}";
        return Ok(result);
    }

    [HttpPut]
    public IActionResult Put([FromQuery] string? name)
    {
        string result = $"Hi, {name}";
        return Ok(result);
    }

    [HttpPatch]
    public IActionResult Patch([FromHeader] string? name)
    {
        string result = $"Good day, {name}";
        return Ok(result);
    }

    [HttpDelete]
    public IActionResult Delete()
    {
        return Ok("Bye.");
    }
}
