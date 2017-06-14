using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceQueryService<T> : IResourceQueryService<T, int> 
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceQueryService<T, in TId> 
        where T : class, IIdentifiable<TId>
    {
        Task<IEnumerable<T>> GetAsync();
        Task<T> GetAsync(TId id);
        Task<object> GetRelationshipsAsync(TId id, string relationshipName);
        Task<object> GetRelationshipAsync(TId id, string relationshipName);
    }
}
