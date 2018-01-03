using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
     public interface IEntityReadRepository<TEntity>
        : IEntityReadRepository<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    { }

    public interface IEntityReadRepository<TEntity, in TId>
        where TEntity : class, IIdentifiable<TId>
    {
        IQueryable<TEntity> Get();

        IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName);

        IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery);

        IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries);

        Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber);

        Task<TEntity> GetAsync(TId id);

        Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName);

        Task<long> CountAsync(IQueryable<TEntity> entities);

        Task<TEntity> FirstOrDefaultAsync(IQueryable<TEntity> entities);

        Task<IReadOnlyList<TEntity>> ToListAsync(IQueryable<TEntity> entities);
    }
}
