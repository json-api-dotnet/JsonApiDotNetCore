using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IAddRelationshipService<TResource> : IAddRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int> { }

    /// <summary />
    public interface IAddRelationshipService<TResource, in TId> where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to add resources to a to-many relationship.
        /// </summary>
        Task AddRelationshipAsync(TId id, string relationshipName, IReadOnlyCollection<IIdentifiable> newValues);
    }
}
