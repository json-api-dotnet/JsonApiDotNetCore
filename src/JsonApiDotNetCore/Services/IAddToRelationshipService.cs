using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IAddToRelationshipService<TResource> : IAddToRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int> { }

    /// <summary />
    public interface IAddToRelationshipService<TResource, in TId> where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to add resources to a to-many relationship.
        /// </summary>
        /// <param name="id">The identifier of the primary resource.</param>
        /// <param name="relationshipName">The relationship to add resources to.</param>
        /// <param name="secondaryResources">The resources to add to the relationship.</param>
        Task AddRelationshipAsync(TId id, string relationshipName, IReadOnlyCollection<IIdentifiable> secondaryResources);
    }
}
