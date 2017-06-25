using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers
{
    [Route("[controller]")]
    public class TestValuesController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var result = new string[] { "value" };
            return Ok(result);
        }
    }
}
