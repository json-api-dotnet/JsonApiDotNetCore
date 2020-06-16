using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging;
using ReportsExample.Models;

namespace ReportsExample.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : BaseJsonApiController<Report> 
    {
        public ReportsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IGetAllService<Report> getAll)
            : base(jsonApiOptions, loggerFactory, getAll)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();
    }
}
