using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomRoutes;

[DisableRoutingConvention]
[Route("world-api/civilization/popular/towns")]
partial class TownsController
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiRequest _request;
    private readonly CustomRouteDbContext _dbContext;

    [ActivatorUtilitiesConstructor]
    public TownsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Town, long> resourceService,
        IJsonApiRequest request, CustomRouteDbContext dbContext)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
        _resourceGraph = resourceGraph;
        _request = request;
        _dbContext = dbContext;
    }

    [HttpGet("largest-{count}")]
    public async Task<IActionResult> GetLargestTownsAsync(int count, CancellationToken cancellationToken)
    {
        IQueryable<Town> query = _dbContext.Towns.OrderByDescending(town => town.Civilians.Count).Take(count);

        List<Town> results = await query.ToListAsync(cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id}/founder")]
    public async Task<IActionResult> GetFounderAsync(long id, CancellationToken cancellationToken)
    {
        var query =
            from town in _dbContext.Towns
            where town.Id == id
            join civilian in _dbContext.Civilians on town.FounderName equals civilian.Name into founders
            from founder in founders.DefaultIfEmpty()
            select new
            {
                Founder = founder
            };

        var result = await query.FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            ResourceType townResourceType = _resourceGraph.GetResourceType<Town>();
            throw new ResourceNotFoundException(id.ToString(), townResourceType.PublicName);
        }

        // Override the controller-level resource type (Town) so the serializer can write a Civilian response.
        ((JsonApiRequest)_request).PrimaryResourceType = _resourceGraph.GetResourceType<Civilian>();

        return Ok(result.Founder);
    }
}
