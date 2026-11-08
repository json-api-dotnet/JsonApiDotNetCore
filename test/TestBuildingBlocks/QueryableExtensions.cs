using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestBuildingBlocks;

public static class QueryableExtensions
{
    public static Task<TResource> FirstWithIdAsync<TResource, TId>(this IQueryable<TResource> resources, TId id)
        where TResource : IIdentifiable<TId>
    {
        return resources.FirstAsync(resource => Equals(resource.Id, id), TestContext.Current.CancellationToken);
    }

    public static async Task<TResource?> FirstWithIdOrDefaultAsync<TResource, TId>(this IQueryable<TResource> resources, TId id)
        where TResource : IIdentifiable<TId>
    {
        return await resources.FirstOrDefaultAsync(resource => Equals(resource.Id, id), TestContext.Current.CancellationToken);
    }
}
