using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#pragma warning disable format

namespace OpenApiTests.MixedControllers;

partial class CupOfCoffeesController
{
    private readonly CoffeeDbContext _dbContext;

    [ActivatorUtilitiesConstructor]
    public CupOfCoffeesController(CoffeeDbContext dbContext, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IGetAllService<CupOfCoffee, long> getAll, IDeleteService<CupOfCoffee, long> delete)
        : base(options, resourceGraph, loggerFactory, getAll, delete: delete)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    [HttpGet("onlyBlack", Name = "get-only-black")]
    [HttpHead("onlyBlack", Name = "head-only-black")]
    [EndpointDescription("Gets all cups of coffee without sugar and milk.")]
    [ProducesResponseType<ICollection<CupOfCoffee>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOnlyBlackAsync(CancellationToken cancellationToken)
    {
        List<CupOfCoffee> cups = await _dbContext.CupsOfCoffee.Where(cup => cup.HasSugar == false && cup.HasMilk == false).ToListAsync(cancellationToken);
        return Ok(cups);
    }

    [HttpGet("onlyBlack/{id}", Name = "get-only-if-black")]
    [HttpHead("onlyBlack/{id}", Name = "head-only-if-black")]
    [EndpointDescription("Gets a cup of coffee by ID, if the cup is without sugar and milk. Returns 404 otherwise.")]
    [ProducesResponseType<CupOfCoffee>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIfOnlyBlackAsync([Required] long id, CancellationToken cancellationToken)
    {
        CupOfCoffee? cup = await _dbContext.CupsOfCoffee.Where(cup => cup.Id == id && cup.HasSugar == false && cup.HasMilk == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (cup == null)
        {
            throw new ResourceNotFoundException(id.ToString(), "cupOfCoffees");
        }

        return Ok(cup);
    }

    [HttpPost("batch", Name = "batchCreateCupsOfCoffee")]
    [EndpointDescription("Creates cups of coffee in batch.")]
    [Consumes(typeof(CupOfCoffee), "application/vnd.api+json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatchAsync([FromQuery] [Required] [Description("The batch size.")] int size,
        [FromBody] [Required] CupOfCoffee template, CancellationToken cancellationToken)
    {
        if (size < 1)
        {
            return Error(new ErrorObject(HttpStatusCode.BadRequest)
            {
                StatusCode = HttpStatusCode.BadRequest,
                Title = "Invalid batch size.",
                Detail = "Please specify a batch size of one or higher in the query string.",
                Source = new ErrorSource
                {
                    Parameter = "size"
                }
            });
        }

        for (int index = 0; index < size; index++)
        {
            var cup = new CupOfCoffee
            {
                HasSugar = template.HasSugar,
                HasMilk = template.HasMilk
            };

            _dbContext.Add(cup);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("batch", Name = "batchResetToBlack")]
    [EndpointDescription("Resets all cups of coffee to black.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetAllToBlackAsync(CancellationToken cancellationToken)
    {
        // @formatter:keep_existing_linebreaks true

        await _dbContext.CupsOfCoffee.ExecuteUpdateAsync(setters => setters
                .SetProperty(cup => cup.HasSugar, false)
                .SetProperty(cup => cup.HasMilk, false),
            cancellationToken);

        // @formatter:keep_existing_linebreaks restore

        return NoContent();
    }

    [HttpDelete("batch", Name = "deleteAll")]
    [EndpointDescription("Deletes all cups of coffee. Returns 404 when none found.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        int numDeleted = await _dbContext.CupsOfCoffee.ExecuteDeleteAsync(cancellationToken);

        if (numDeleted == 0)
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.NotFound));
        }

        Response.StatusCode = (int)HttpStatusCode.NoContent;
    }
}
