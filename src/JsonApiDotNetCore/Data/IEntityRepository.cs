using System.Linq;
using System.Threading.Tasks;
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

        Task<TEntity> GetAsync(TId id);

        Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName);

        Task<TEntity> CreateAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TId id, TEntity entity);

        Task<bool> DeleteAsync(TId id);
    }
}
