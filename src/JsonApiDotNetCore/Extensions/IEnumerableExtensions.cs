using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// gets the first element of type <typeparamref name="TImplementedService"/> if it exists and casts the result to that.
        /// Returns null otherwise.
        /// </summary>
        public static TImplementedService FirstOrDefault<TImplementedService>(this IEnumerable<IQueryParameterService> data) where TImplementedService : class, IQueryParameterService
        {
            return data.FirstOrDefault(qp => qp is TImplementedService) as TImplementedService;
        }
    }
}
