using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public interface IUpdateRelationshipService<TResource> : IUpdateRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    /// <summary />
    public interface IUpdateRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Handles a json:api request to update an existing relationship.
        /// </summary>
        Task UpdateRelationshipAsync(TId id, string relationshipName, object relationships);
    }
}
