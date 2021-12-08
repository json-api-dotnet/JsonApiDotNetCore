using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services;

/// <summary />
public interface ICreateService<TResource, in TId>
    where TResource : class, IIdentifiable<TId>
{
    /// <summary>
    /// Handles a JSON:API request to create a new resource with attributes, relationships or both.
    /// </summary>
    Task<TResource?> CreateAsync(TResource resource, CancellationToken cancellationToken);
}
