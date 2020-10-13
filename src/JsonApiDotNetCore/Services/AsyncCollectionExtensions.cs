using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.Services
{
    public static class AsyncCollectionExtensions
    {
        public static async Task AddRangeAsync<T>(this ICollection<T> source, IAsyncEnumerable<T> elementsToAdd)
        {
            await foreach (var missingResource in elementsToAdd)
            {
                source.Add(missingResource);
            }
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var list = new List<T>();

            await foreach (var element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
}
