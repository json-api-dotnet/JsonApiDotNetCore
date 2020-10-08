using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IDeleteRelationshipService<TResource> : IDeleteRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int> { }

    /// <summary />
    public interface IDeleteRelationshipService<TResource, in TId> where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to remove resources from a to-many relationship.
        /// </summary>
        Task DeleteRelationshipAsync(TId id, string relationshipName, IReadOnlyCollection<IIdentifiable> removalValues);
    }
}
