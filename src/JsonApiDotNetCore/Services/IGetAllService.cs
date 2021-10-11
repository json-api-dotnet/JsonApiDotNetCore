#nullable disable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <summary />
    public interface IGetAllService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to retrieve a collection of resources for a primary endpoint.
        /// </summary>
        Task<IReadOnlyCollection<TResource>> GetAsync(CancellationToken cancellationToken);
    }
}
