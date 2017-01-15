using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Data
{
    public class DefaultEntityRepository<TEntity, TId> 
        : IEntityRepository<TEntity, TId> 
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly ILogger _logger;
        private readonly IJsonApiContext _jsonApiContext;

        public DefaultEntityRepository(
            DbContext context,
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext)
        {
            _context = context;
            _dbSet = context.GetDbSet<TEntity>();
            _jsonApiContext = jsonApiContext;
            _logger = loggerFactory.CreateLogger<DefaultEntityRepository<TEntity, TId>>();
        }

        public IQueryable<TEntity> Get()
        {
            return _dbSet;
        }

        public async Task<TEntity> GetAsync(TId id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public async Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            return await _dbSet
                .Include(relationshipName)
                .FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TId id, TEntity entity)
        {
            var oldEntity = await GetAsync(id);

            if (oldEntity == null)
                return null;

            _jsonApiContext.RequestEntity.Attributes.ForEach(attr =>
            {
                attr.SetValue(oldEntity, attr.GetValue(entity));
            });

            await _context.SaveChangesAsync();

            return oldEntity;
        }

        public async Task<bool> DeleteAsync(TId id)
        {
            var entity = await GetAsync(id);

            if (entity == null)
                return false;

            _dbSet.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
