using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IGetRelationshipService<TResource> : IGetRelationshipService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IGetRelationshipService<TResource, in TId>
        where TResource : class, IIdentifiable<TId>
    {
        Task<TResource> GetRelationshipAsync(TId id, string relationshipName);
    }
}
