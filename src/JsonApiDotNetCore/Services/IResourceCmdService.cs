using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceCmdService<T> : IResourceCmdService<T, int> 
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceCmdService<T, in TId> 
        where T : class, IIdentifiable<TId>
    {
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(TId id, T entity);
        Task UpdateRelationshipsAsync(TId id, string relationshipName, List<DocumentData> relationships);
        Task<bool> DeleteAsync(TId id);
    }
}
