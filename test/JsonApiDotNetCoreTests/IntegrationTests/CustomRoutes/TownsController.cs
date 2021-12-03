using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes
{
    // Workaround for https://youtrack.jetbrains.com/issue/RSRP-487028
    public partial class TownsController
    {
    }

    [DisableRoutingConvention]
    [Route("world-api/civilization/popular/towns")]
    partial class TownsController
    {
        private readonly CustomRouteDbContext _dbContext;

        [ActivatorUtilitiesConstructor]
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
