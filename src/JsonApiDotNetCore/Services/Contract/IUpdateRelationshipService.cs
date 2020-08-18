using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IUpdateRelationshipService<TResource> : IUpdateRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IUpdateRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task UpdateRelationshipAsync(TId id, string relationshipName, object relationships);
    }
}
