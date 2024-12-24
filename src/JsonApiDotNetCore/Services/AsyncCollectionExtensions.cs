using JetBrains.Annotations;

namespace JsonApiDotNetCore.Services;

[PublicAPI]
public static class AsyncCollectionExtensions
{
    public static async Task AddRangeAsync<T>(this ICollection<T> source, IAsyncEnumerable<T> elementsToAdd, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(elementsToAdd);

        await foreach (T missingResource in elementsToAdd.WithCancellation(cancellationToken))
        {
            source.Add(missingResource);
        }
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        List<T> list = [];

        await foreach (T element in source.WithCancellation(cancellationToken))
        {
            list.Add(element);
        }

        return list;
    }
}
