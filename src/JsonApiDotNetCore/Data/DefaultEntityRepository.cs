using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Data
{
    public class DefaultEntityRepository<TEntity> 
        : DefaultEntityRepository<TEntity, int>,
        IEntityRepository<TEntity> 
        where TEntity : class, IIdentifiable<int>
    {
        public DefaultEntityRepository(
            DbContext context,
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext)
        : base(context, loggerFactory, jsonApiContext)
        { }
    }

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

        public virtual IQueryable<TEntity> Get()
        {
            return _dbSet;
        }

        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> entities,  FilterQuery filterQuery)
        {
            if(filterQuery == null)
                return entities;

            return entities
                .Filter(filterQuery);
        }

        public virtual IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries)
        {
            if(sortQueries == null || sortQueries.Count == 0)
                return entities;

            var orderedEntities = entities.Sort(sortQueries[0]);

            if(sortQueries.Count() > 1)
                for(var i=1; i < sortQueries.Count(); i++)
                    orderedEntities = orderedEntities.Sort(sortQueries[i]);

            return orderedEntities;
        }

        public virtual async Task<TEntity> GetAsync(TId id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual async Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            return await _dbSet
                .Include(relationshipName)
                .FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TId id, TEntity entity)
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

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = await GetAsync(id);

            if (entity == null)
                return false;

            _dbSet.Remove(entity);

            await _context.SaveChangesAsync();

            return true;
        }

        public IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName)
        {
            var entity = _jsonApiContext.RequestEntity;
            if(entity.Relationships.Any(r => r.RelationshipName == relationshipName))
                return entities.Include(relationshipName);

            throw new JsonApiException("400", "Invalid relationship",
                $"{entity.EntityName} does not have a relationship named {relationshipName}");
        }
    }
}
