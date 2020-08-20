using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers.Restricted
{
    [DisableRoutingConvention, Route("[controller]")]
    [HttpReadOnly]
    public class ReadOnlyController : BaseJsonApiController<Article>
    {
        public ReadOnlyController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [DisableRoutingConvention, Route("[controller]")]
    [NoHttpPost]
    public class NoHttpPostController : BaseJsonApiController<Article>
    {
        public NoHttpPostController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [DisableRoutingConvention, Route("[controller]")]
    [NoHttpPatch]
    public class NoHttpPatchController : BaseJsonApiController<Article>
    {
        public NoHttpPatchController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

        [HttpGet]
        public IActionResult Get() => Ok();

        [HttpPost]
        public IActionResult Post() => Ok();

        [HttpPatch]
        public IActionResult Patch() => Ok();

        [HttpDelete]
        public IActionResult Delete() => Ok();
    }

    [DisableRoutingConvention, Route("[controller]")]
    [NoHttpDelete]
    public class NoHttpDeleteController : BaseJsonApiController<Article>
    {
        public NoHttpDeleteController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Article> resourceService) 
            : base(options, loggerFactory, resourceService)
        { }

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
