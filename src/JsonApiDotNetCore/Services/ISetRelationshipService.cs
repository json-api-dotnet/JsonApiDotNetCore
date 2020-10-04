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
        /// Handles a json:api request to update an existing relationship.
        /// </summary>
        Task SetRelationshipAsync(TId id, string relationshipName, object relationships);
    }
}
