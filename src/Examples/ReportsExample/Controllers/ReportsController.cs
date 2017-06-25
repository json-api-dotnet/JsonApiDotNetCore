using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace ReportsExample.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : BaseJsonApiController<Report, int> 
    {
        public ReportsController(
            IJsonApiContext jsonApiContext, 
            IGetAllService<Report> getAll)
        : base(jsonApiContext, getAll: getAll)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();
    }
}
