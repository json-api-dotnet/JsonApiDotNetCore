using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface ICreateService<TResource> : ICreateService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface ICreateService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to create a new resource with attributes, relationships or both.
        /// </summary>
        Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken);
    }
}
