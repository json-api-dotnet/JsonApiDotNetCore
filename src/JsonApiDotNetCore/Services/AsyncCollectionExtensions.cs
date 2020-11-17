using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.Services
{
    public static class AsyncCollectionExtensions
    {
        public static async Task AddRangeAsync<T>(this ICollection<T> source, IAsyncEnumerable<T> elementsToAdd, CancellationToken cancellationToken = default)
        {
            await foreach (var missingResource in elementsToAdd.WithCancellation(cancellationToken))
            {
                source.Add(missingResource);
            }
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var list = new List<T>();

            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }
    }
}
