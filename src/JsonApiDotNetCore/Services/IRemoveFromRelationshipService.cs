using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IRemoveFromRelationshipService<TResource> : IRemoveFromRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface IRemoveFromRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to remove resources from a to-many relationship.
        /// </summary>
        /// <param name="primaryId">The identifier of the primary resource.</param>
        /// <param name="relationshipName">The relationship to remove resources from.</param>
        /// <param name="secondaryResourceIds">The set of resources to remove from the relationship.</param>
        /// <param name="cancellationToken">Propagates notification that request handling should be canceled.</param>
        Task RemoveFromToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken);
    }
}
