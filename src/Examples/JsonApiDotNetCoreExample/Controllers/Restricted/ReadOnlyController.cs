using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers.Restricted
{
    [Route("[controller]")]
    [HttpReadOnly]
    public class ReadOnlyController : ControllerBase
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
    public class NoHttpPostController : ControllerBase
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
    public class NoHttpPatchController : ControllerBase
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
    public class NoHttpDeleteController : ControllerBase
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
