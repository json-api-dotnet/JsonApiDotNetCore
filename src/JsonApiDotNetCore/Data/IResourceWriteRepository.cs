using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
     public interface IResourceWriteRepository<TEntity>
        : IResourceWriteRepository<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    { }

    public interface IResourceWriteRepository<TEntity, in TId>
        where TEntity : class, IIdentifiable<TId>
    {
        Task<TEntity> CreateAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);

        Task<bool> DeleteAsync(TId id);
    }
}
