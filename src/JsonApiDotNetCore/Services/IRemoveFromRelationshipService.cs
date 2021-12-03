using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.Services
{
    /// <summary />
    public interface IRemoveFromRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a JSON:API request to remove resources from a to-many relationship.
        /// </summary>
        /// <param name="leftId">
        /// Identifies the left side of the relationship.
        /// </param>
        /// <param name="relationshipName">
        /// The relationship to remove resources from.
        /// </param>
        /// <param name="rightResourceIds">
        /// The set of resources to remove from the relationship.
        /// </param>
        /// <param name="cancellationToken">
        /// Propagates notification that request handling should be canceled.
        /// </param>
        Task RemoveFromToManyRelationshipAsync(TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken);
    }
}
