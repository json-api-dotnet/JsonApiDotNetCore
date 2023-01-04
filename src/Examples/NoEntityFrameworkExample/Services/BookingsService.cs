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
    public BookingsService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext,
        IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request, IResourceChangeTracker<Booking> resourceChangeTracker,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
        : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker, resourceDefinitionAccessor)
    {
    }

    public override async Task<IReadOnlyCollection<Booking>> GetAsync(CancellationToken cancellationToken)
    {
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
