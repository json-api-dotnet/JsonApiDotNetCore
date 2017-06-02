using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers.Restricted
{
    [Route("[controller]")]
    [HttpReadOnly]
    public class ReadOnlyController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [Route("[controller]")]
    [NoHttpPost]
    public class NoHttpPostController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [Route("[controller]")]
    [NoHttpPatch]
    public class NoHttpPatchController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [Route("[controller]")]
    [NoHttpDelete]
    public class NoHttpDeleteController : Controller
    {
        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }
}
