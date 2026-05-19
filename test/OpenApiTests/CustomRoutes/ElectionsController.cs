using System.ComponentModel.DataAnnotations;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.CustomRoutes;

[DisableRoutingConvention]
[Route("voting-api/overview")]
partial class ElectionsController
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiRequest _request;
    private readonly CustomRouteDbContext _dbContext;

    [ActivatorUtilitiesConstructor]
    public ElectionsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Election, Guid> resourceService, IJsonApiRequest request, CustomRouteDbContext dbContext)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
        _resourceGraph = resourceGraph;
        _request = request;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the candidate with the most votes for a given election.
    /// </summary>
    /// <param name="id">
    /// The identifier of the election.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that request handling should be canceled.
    /// </param>
    /// <response code="200">
    /// Successfully returns the winner.
    /// </response>
    /// <response code="404">
    /// The election does not exist.
    /// </response>
    /// <response code="409">
    /// No single winner found.
    /// </response>
    [HttpHead("winner/{id}", Name = "head-winner")]
    [HttpGet("winner/{id}", Name = "get-winner")]
    [ProducesResponseType<Candidate>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetWinnerAsync([Required] Guid id, CancellationToken cancellationToken)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        var topCandidates = await _dbContext.Ballots
            .Where(ballot => ballot.Election.Id == id && ballot.SelectedCandidate != null)
            .GroupBy(ballot => ballot.SelectedCandidate)
            .Select(ballotsPerCandidate => new
            {
                Candidate = ballotsPerCandidate.Key!,
                VoteCount = ballotsPerCandidate.Count()
            })
            .OrderByDescending(candidateWithVotes => candidateWithVotes.VoteCount)
            .Take(2)
            .ToArrayAsync(cancellationToken);

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        if (topCandidates.Length == 0)
        {
            Election? election = await _dbContext.Elections.FirstOrDefaultAsync(election => election.Id == id, cancellationToken);

            if (election == null)
            {
                return Error(new ErrorObject(HttpStatusCode.NotFound)
                {
                    Title = "The election does not exist.",
                    Detail = $"Election with ID '{id}' does not exist."
                });
            }
        }
        else if (topCandidates.Length == 1 || topCandidates[0].VoteCount > topCandidates[1].VoteCount)
        {
            // Override the controller-level resource type so the serializer can write a Candidate response.
            ((JsonApiRequest)_request).PrimaryResourceType = _resourceGraph.GetResourceType<Candidate>();

            Candidate winner = topCandidates[0].Candidate;
            return Ok(winner);
        }

        return Error(new ErrorObject(HttpStatusCode.Conflict)
        {
            Title = "No single winner found.",
            Detail = topCandidates.Length == 0 ? "There are no votes." : "Multiple candidates are tied for first place."
        });
    }
}
