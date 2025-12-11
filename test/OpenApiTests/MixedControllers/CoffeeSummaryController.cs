using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.MixedControllers;

public sealed class CoffeeSummaryController : BaseJsonApiController<CoffeeSummary, long>
{
    private readonly CoffeeDbContext _dbContext;

    public CoffeeSummaryController(CoffeeDbContext dbContext, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory)
        : base(options, resourceGraph, loggerFactory, null, null)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    [HttpGet("summary", Name = "get-coffee-summary")]
    [HttpHead("summary", Name = "head-coffee-summary")]
    [EndpointDescription("Summarizes all cups of coffee, indicating their ingredients.")]
    [ProducesResponseType<CoffeeSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var summary = new CoffeeSummary
        {
            Id = 1
        };

        foreach (CupOfCoffee cupOfCoffee in await _dbContext.CupsOfCoffee.ToArrayAsync(cancellationToken))
        {
            bool hasSugar = cupOfCoffee.HasSugar.GetValueOrDefault();
            bool hasMilk = cupOfCoffee.HasMilk.GetValueOrDefault();

            switch (hasSugar, hasMilk)
            {
                case (false, false):
                {
                    summary.BlackCount++;
                    break;
                }
                case (false, true):
                {
                    summary.OnlyMilkCount++;
                    break;
                }
                case (true, false):
                {
                    summary.OnlySugarCount++;
                    break;
                }
                case (true, true):
                {
                    summary.SugarWithMilkCount++;
                    break;
                }
            }

            summary.TotalCount++;
        }

        if (summary.TotalCount == 0)
        {
            return Error(new ErrorObject(HttpStatusCode.NotFound)
            {
                StatusCode = HttpStatusCode.NotFound,
                Title = "No cups available to summarize."
            });
        }

        return Ok(summary);
    }
}
