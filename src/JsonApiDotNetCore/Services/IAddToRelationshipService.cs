using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IAddToRelationshipService<TResource> : IAddToRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface IAddToRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to add resources to a to-many relationship.
        /// </summary>
        /// <param name="primaryId">The identifier of the primary resource.</param>
        /// <param name="relationshipName">The relationship to add resources to.</param>
        /// <param name="secondaryResourceIds">The set of resources to add to the relationship.</param>
        /// <param name="cancellationToken">Propagates notification that request handling should be canceled.</param>
        Task AddToToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken);
    }
}
