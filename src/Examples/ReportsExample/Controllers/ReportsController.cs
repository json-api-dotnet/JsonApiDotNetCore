using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportsExample.Models;

namespace ReportsExample.Controllers
{
    [Route("api/[controller]")]
    public class ReportsController : BaseJsonApiController<Report> 
    {
        public ReportsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IGetAllService<Report> getAll)
            : base(options, loggerFactory, getAll)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            return await base.GetAsync(cancellationToken);
        }
    }
}
