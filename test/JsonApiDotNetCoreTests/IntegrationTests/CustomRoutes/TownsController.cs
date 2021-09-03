using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    [DisableRoutingConvention]
    [Route("world-api/civilization/popular/towns")]
    public sealed class TownsController : JsonApiController<Town>
    {
        private readonly CustomRouteDbContext _dbContext;

        public TownsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Town> resourceService, CustomRouteDbContext dbContext)
            : base(options, loggerFactory, resourceService)
        {
            _dbContext = dbContext;
        }

        [HttpGet("largest-{count}")]
        public async Task<IActionResult> GetLargestTownsAsync(int count, CancellationToken cancellationToken)
        {
            IQueryable<Town> query = _dbContext.Towns.OrderByDescending(town => town.Civilians.Count).Take(count);

            List<Town> results = await query.ToListAsync(cancellationToken);
            return Ok(results);
        }
    }
}
