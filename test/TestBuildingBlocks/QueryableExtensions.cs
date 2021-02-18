using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;

namespace TestBuildingBlocks
{
    public static class QueryableExtensions
    {
        public static Task<TResource> FirstWithIdAsync<TResource, TId>(this IQueryable<TResource> resources, TId id,
            CancellationToken cancellationToken = default)
            where TResource : IIdentifiable<TId>
        {
            return resources.FirstAsync(resource => Equals(resource.Id, id), cancellationToken);
        }

        public static Task<TResource> FirstWithIdOrDefaultAsync<TResource, TId>(this IQueryable<TResource> resources, TId id,
            CancellationToken cancellationToken = default)
            where TResource : IIdentifiable<TId>
        {
            return resources.FirstOrDefaultAsync(resource => Equals(resource.Id, id), cancellationToken);
        }
    }
}
