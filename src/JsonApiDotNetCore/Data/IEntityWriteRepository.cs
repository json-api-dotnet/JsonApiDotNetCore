using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Data
{
     public interface IEntityWriteRepository<TEntity>
        : IEntityWriteRepository<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    { }

    public interface IEntityWriteRepository<TEntity, in TId>
        where TEntity : class, IIdentifiable<TId>
    {
        Task<TEntity> CreateAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TId id, TEntity entity);

        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);

        Task<bool> DeleteAsync(TId id);
    }
}
