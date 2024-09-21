using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[Route("[controller]")]
public sealed class NonJsonApiController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        string[] result = ["Welcome!"];
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync()
    {
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        string name = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Please send your name.");
        }

        string result = $"Hello, {name}";
        return Ok(result);
    }

    [HttpPut]
    public IActionResult Put([FromBody] string name)
    {
        string result = $"Hi, {name}";
        return Ok(result);
    }

    [HttpPatch]
    public IActionResult Patch(string name)
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
