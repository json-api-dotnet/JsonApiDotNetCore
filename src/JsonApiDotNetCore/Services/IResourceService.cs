using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceService<T> : IResourceService<T, int> 
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceService<T, in TId> 
        where T : class, IIdentifiable<TId>
    {
        Task<IEnumerable<T>> GetAsync();
        Task<T> GetAsync(TId id);
        Task<object> GetRelationshipsAsync(TId id, string relationshipName);
        Task<object> GetRelationshipAsync(TId id, string relationshipName);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(TId id, T entity);
        Task UpdateRelationshipsAsync(TId id, string relationshipName, List<DocumentData> relationships);
        Task<bool> DeleteAsync(TId id);
    }
}
