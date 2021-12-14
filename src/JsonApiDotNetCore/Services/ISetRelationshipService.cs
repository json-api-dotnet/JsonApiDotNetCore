using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services;

/// <summary />
public interface ISetRelationshipService<TResource, in TId>
    where TResource : class, IIdentifiable<TId>
{
    /// <summary>
    /// Handles a JSON:API request to perform a complete replacement of a relationship on an existing resource.
    /// </summary>
    /// <param name="leftId">
    /// Identifies the left side of the relationship.
    /// </param>
    /// <param name="relationshipName">
    /// The relationship for which to perform a complete replacement.
    /// </param>
    /// <param name="rightValue">
    /// The resource or set of resources to assign to the relationship.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that request handling should be canceled.
    /// </param>
    Task SetRelationshipAsync(TId leftId, string relationshipName, object? rightValue, CancellationToken cancellationToken);
}
