using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IDeleteService<TResource> : IDeleteService<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary />
    public interface IDeleteService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to delete an existing resource.
        /// </summary>
        Task DeleteAsync(TId id, CancellationToken cancellationToken);
    }
}
