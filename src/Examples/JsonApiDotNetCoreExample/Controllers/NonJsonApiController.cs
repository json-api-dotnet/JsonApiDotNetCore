using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers;

[Route("[controller]")]
[Tags("nonJsonApi")]
public sealed class NonJsonApiController : ControllerBase
{
    [HttpGet(Name = "welcomeGet")]
    [HttpHead(Name = "welcomeHead")]
    [EndpointDescription("Returns a single-element JSON array.")]
    [ProducesResponseType<List<string>>(StatusCodes.Status200OK, "application/json")]
    public IActionResult Get()
    {
        string[] result = ["Welcome!"];

        return Ok(result);
    }

    [HttpPost]
    [EndpointDescription("Returns a greeting text, based on your name.")]
    [Consumes("application/json")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "text/plain")]
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
    [EndpointDescription("Returns another greeting text.")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
    public IActionResult Put([FromQuery] string? name)
    {
        string result = $"Hi, {name}";
        return Ok(result);
    }

    [HttpPatch]
    [EndpointDescription("Wishes you a good day.")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/plain")]
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
