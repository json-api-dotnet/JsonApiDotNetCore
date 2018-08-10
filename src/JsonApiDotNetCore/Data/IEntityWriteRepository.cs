using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

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

        /// <summary>
        /// Ensures that any relationship pointers created during a POST or PATCH
        /// request are detached from the DbContext.
        /// This allows the relationships to be fully loaded from the database.
        /// 
        /// </summary>
        /// <remarks>
        /// The only known case when this should be called is when a POST request is
        /// sent with an ?include query.
        /// 
        /// See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
        /// </remarks>
        void DetachRelationshipPointers(TEntity entity);
    }
}
