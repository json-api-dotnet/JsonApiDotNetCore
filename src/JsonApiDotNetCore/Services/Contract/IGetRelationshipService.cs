using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IGetRelationshipService<T> : IGetRelationshipService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IGetRelationshipService<T, in TId>
        where T : class, IIdentifiable<TId>
    {
        Task<object> GetRelationshipAsync(TId id, string relationshipName);
    }
}
