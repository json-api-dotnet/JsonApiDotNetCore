using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IGetByIdService<TResource> : IGetByIdService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface IGetByIdService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to retrieve a single resource for a primary endpoint.
        /// </summary>
        Task<TResource> GetAsync(TId id, CancellationToken cancellationToken);
    }
}
