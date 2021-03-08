using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Services
{
    [PublicAPI]
    public static class AsyncCollectionExtensions
    {
        public static async Task AddRangeAsync<T>(this ICollection<T> source, IAsyncEnumerable<T> elementsToAdd, CancellationToken cancellationToken = default)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(elementsToAdd, nameof(elementsToAdd));

            await foreach (T missingResource in elementsToAdd.WithCancellation(cancellationToken))
            {
                source.Add(missingResource);
            }
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            ArgumentGuard.NotNull(source, nameof(source));

            var list = new List<T>();

            await foreach (T element in source.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }
    }
}
