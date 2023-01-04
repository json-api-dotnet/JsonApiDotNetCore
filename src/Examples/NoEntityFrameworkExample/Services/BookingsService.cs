using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Services;

// This is the implementation of BookingService.
public class BookingsService : JsonApiResourceService<Booking, string>
{
    private readonly IQueryLayerComposer _queryLayerComposer;
    private readonly IJsonApiRequest _request;

    public BookingsService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext,
        IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request, IResourceChangeTracker<Booking> resourceChangeTracker,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
        : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker, resourceDefinitionAccessor)
    {
        _queryLayerComposer = queryLayerComposer;
        _request = request;
    }

    public override async Task<IReadOnlyCollection<Booking>> GetAsync(CancellationToken cancellationToken)
    {
        _queryLayerComposer.ComposeFromConstraints(_request.PrimaryResourceType!);

        return new List<Booking>
        {
            new()
            {
                Id = "1",
                Title = "booking 1",
                Spaces = new List<Space>
                {
                    new()
                    {
                        Id = "1",
                        Title = "test space"
                    }
                }
            },
            new()
            {
                Id = "2",
                Title = "booking 2",
                Spaces = new List<Space>
                {
                    new()
                    {
                        Id = "1",
                        Title = "test space"
                    }
                }
            }
        };
    }
}
