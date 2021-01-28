using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers
{
    [Route("[controller]")]
    public sealed class NonJsonApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var result = new[] {"Welcome!"};
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            string name = await new StreamReader(Request.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Please send your name.");
            }

            var result = "Hello, " + name;
            return Ok(result);
        }

        [HttpPut]
        public IActionResult Put([FromBody] string name)
        {
            var result = "Hi, " + name;
            return Ok(result);
        }

        [HttpPatch]
        public IActionResult Patch(string name)
        {
            var result = "Good day, " + name;
            return Ok(result);
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            return Ok("Bye.");
        }
    }
}
