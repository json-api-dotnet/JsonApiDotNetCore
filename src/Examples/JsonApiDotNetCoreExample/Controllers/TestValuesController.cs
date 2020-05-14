using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers
{
    [Route("[controller]")]
    public class TestValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var result = new[] { "value" };
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post(string name)
        {
            var result = "Hello, " + name;
            return Ok(result);
        }

        [HttpPatch]
        public IActionResult Patch(string name)
        {
            var result = "Hello, " + name;
            return Ok(result);
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            return Ok("Deleted");
        }
    }
}
