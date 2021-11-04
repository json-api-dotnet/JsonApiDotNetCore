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

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    [DisableRoutingConvention]
    [Route("world-api/civilization/popular/towns")]
    public sealed class TownsController : JsonApiController<Town, int>
    {
        private readonly CustomRouteDbContext _dbContext;

        public TownsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Town, int> resourceService,
            CustomRouteDbContext dbContext)
            : base(options, resourceGraph, loggerFactory, resourceService)
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
