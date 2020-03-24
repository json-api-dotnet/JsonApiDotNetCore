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
    }
}
