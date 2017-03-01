using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
     public interface IEntityRepository<TEntity>
        : IEntityRepository<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    {
    }

    public interface IEntityRepository<TEntity, in TId>
        where TEntity : class, IIdentifiable<TId>
    {
        IQueryable<TEntity> Get();

        IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName);

        IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery);

        IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries);

        Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber);

        Task<TEntity> GetAsync(TId id);

        Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName);

        Task<TEntity> CreateAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TId id, TEntity entity);

        Task UpdateRelationshipsAsync(object parent, Relationship relationship, IEnumerable<string> relationshipIds);

        Task<bool> DeleteAsync(TId id);
    }
}
