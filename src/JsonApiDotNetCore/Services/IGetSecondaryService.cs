using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services
{
    /// <summary />
    public interface IGetSecondaryService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to retrieve a single resource or a collection of resources for a secondary endpoint, such as /articles/1/author or
        /// /articles/1/revisions.
        /// </summary>
        Task<object?> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken);
    }
}
