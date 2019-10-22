using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;

namespace ReportsExample.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : BaseJsonApiController<Report, int> 
    {
        public ReportsController(
            IJsonApiOptions jsonApiOptions,
            IGetAllService<Report> getAll)
        : base(jsonApiOptions, getAll: getAll)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();
    }
}
