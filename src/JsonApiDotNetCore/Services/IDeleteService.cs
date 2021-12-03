using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services
{
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
