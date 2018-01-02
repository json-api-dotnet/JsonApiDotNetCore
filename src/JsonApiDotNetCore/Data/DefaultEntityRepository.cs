using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
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
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver)
        : base(loggerFactory, jsonApiContext, contextResolver)
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
        private readonly IGenericProcessorFactory _genericProcessorFactory;

        public DefaultEntityRepository(
            ILoggerFactory loggerFactory,
            IJsonApiContext jsonApiContext,
            IDbContextResolver contextResolver)
        {
            _context = contextResolver.GetContext();
            _dbSet = contextResolver.GetDbSet<TEntity>();
            _jsonApiContext = jsonApiContext;
            _logger = loggerFactory.CreateLogger<DefaultEntityRepository<TEntity, TId>>();
            _genericProcessorFactory = _jsonApiContext.GenericProcessorFactory;
        }

        public virtual IQueryable<TEntity> Get()
        {
            if (_jsonApiContext.QuerySet?.Fields != null && _jsonApiContext.QuerySet.Fields.Any())
                return _dbSet.Select(_jsonApiContext.QuerySet?.Fields);

            return _dbSet;
        }

        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> entities, FilterQuery filterQuery)
        {
            return entities.Filter(_jsonApiContext, filterQuery);
        }

        public virtual IQueryable<TEntity> Sort(IQueryable<TEntity> entities, List<SortQuery> sortQueries)
        {
            return entities.Sort(sortQueries);
        }

        public virtual async Task<TEntity> GetAsync(TId id)
        {
            return await Get().SingleOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual async Task<TEntity> GetAndIncludeAsync(TId id, string relationshipName)
        {
            _logger.LogDebug($"[JADN] GetAndIncludeAsync({id}, {relationshipName})");

            var result = await Get()
                .Include(relationshipName)
                .SingleOrDefaultAsync(e => e.Id.Equals(id));

            return result;
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

            foreach (var attr in _jsonApiContext.AttributesToUpdate)
                attr.Key.SetValue(oldEntity, attr.Value);

            foreach (var relationship in _jsonApiContext.RelationshipsToUpdate)
                relationship.Key.SetValue(oldEntity, relationship.Value);

            await _context.SaveChangesAsync();

            return oldEntity;
        }

        public async Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds)
        {
            var genericProcessor = _genericProcessorFactory.GetProcessor(relationship.Type);
            await genericProcessor.UpdateRelationshipsAsync(parent, relationship, relationshipIds);
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

        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> entities, string relationshipName)
        {
            var entity = _jsonApiContext.RequestEntity;
            var relationship = entity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == relationshipName);
            if (relationship != null)
                return entities.Include(relationship.InternalRelationshipName);

            throw new JsonApiException(400, $"Invalid relationship {relationshipName} on {entity.EntityName}",
                $"{entity.EntityName} does not have a relationship named {relationshipName}");
        }

        public virtual async Task<IEnumerable<TEntity>> PageAsync(IQueryable<TEntity> entities, int pageSize, int pageNumber)
        {
            if (pageNumber >= 0)
            {
                return await entities.PageForward(pageSize, pageNumber).ToListAsync();
            }

            // since EntityFramework does not support IQueryable.Reverse(), we need to know the number of queried entities
            int numberOfEntities = await this.CountAsync(entities);

            // may be negative
            int virtualFirstIndex = numberOfEntities - pageSize * Math.Abs(pageNumber);
            int numberOfElementsInPage = Math.Min(pageSize, virtualFirstIndex + pageSize);

            return await entities
                    .Skip(virtualFirstIndex)
                    .Take(numberOfElementsInPage)
                    .ToListAsync();
        }

        public async Task<int> CountAsync(IQueryable<TEntity> entities)
        {
            return await entities.CountAsync();
        }

        public Task<TEntity> FirstOrDefaultAsync(IQueryable<TEntity> entities)
        {
            return entities.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<TEntity>> ToListAsync(IQueryable<TEntity> entities)
        {
            return await entities.ToListAsync();
        }
    }
}
