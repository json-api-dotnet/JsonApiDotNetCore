using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface ISetRelationshipService<TResource> : ISetRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface ISetRelationshipService<TResource, in TId> where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to perform a complete replacement of the value of a relationship.
        /// </summary>
        /// <param name="id">The identifier of the primary resource.</param>
        /// <param name="relationshipName">The relationship for which to perform a complete replacement.</param>
        /// <param name="secondaryResources">The resources to perform the complete replacement with.</param>
        Task SetRelationshipAsync(TId id, string relationshipName, object secondaryResources);
    }
}
