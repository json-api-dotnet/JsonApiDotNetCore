using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    public class MappingResourceService<TResource, TEntity> 
        : MappingResourceService<TResource, TEntity, int>,
        IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
        where TEntity : class, IIdentifiable<int>
    {
        public MappingResourceService(
          IJsonApiContext jsonApiContext,
          IEntityRepository<TEntity> entityRepository,
          ILoggerFactory loggerFactory,
          IMapper mapper)
          : base(jsonApiContext, entityRepository, loggerFactory, mapper)
        { }
    }

    public class MappingResourceService<TResource, TEntity, TId> 
        : IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
        where TEntity : class, IIdentifiable<TId>
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IEntityRepository<TEntity, TId> _entities;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public MappingResourceService(
                IJsonApiContext jsonApiContext,
                IEntityRepository<TEntity, TId> entityRepository,
                ILoggerFactory loggerFactory,
                IMapper mapper)
        {
            _jsonApiContext = jsonApiContext;
            _entities = entityRepository;
            _logger = loggerFactory.CreateLogger<MappingResourceService<TResource, TEntity, TId>>();
            _mapper = mapper;
        }

        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
            var entities = _entities.Get();

            entities = ApplySortAndFilterQuery(entities);

            if (ShouldIncludeRelationships())
                entities = IncludeRelationships(entities, _jsonApiContext.QuerySet.IncludedRelationships);

            if (_jsonApiContext.Options.IncludeTotalRecordCount)
                _jsonApiContext.PageManager.TotalRecords = await _entities.CountAsync(entities);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entities);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            TResource dto;
            if (ShouldIncludeRelationships())
                dto = await GetWithRelationshipsAsync(id);
            else
            {
                TEntity entity = await _entities.GetAsync(id);
                dto = _mapper.Map<TResource>(entity);
            }
            return dto;
        }

        private bool ShouldIncludeRelationships()
            => (_jsonApiContext.QuerySet?.IncludedRelationships != null && _jsonApiContext.QuerySet.IncludedRelationships.Count > 0);

        private async Task<TResource> GetWithRelationshipsAsync(TId id)
        {
            var query = _entities.Get().Where(e => e.Id.Equals(id));
            _jsonApiContext.QuerySet.IncludedRelationships.ForEach(r =>
            {
                query = _entities.Include(query, r);
            });
            var value = await _entities.FirstOrDefaultAsync(query);
            return _mapper.Map<TResource>(value);
        }

        public virtual async Task<object> GetRelationshipsAsync(TId id, string relationshipName)
            => await GetRelationshipAsync(id, relationshipName);

        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);
            // TODO: it would be better if we could distinguish whether or not the relationship was not found,
            // vs the relationship not being set on the instance of T
            if (entity == null)
            {
                throw new JsonApiException(404, $"Relationship {relationshipName} not found.");
            }

            var resource = _mapper.Map<TResource>(entity);
            var relationship = _jsonApiContext.ContextGraph.GetRelationship(resource, relationshipName);
            return relationship;
        }

        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            var entity = _mapper.Map<TEntity>(resource);
            entity = await _entities.CreateAsync(entity);
            return _mapper.Map<TResource>(entity);
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource)
        {
            var entity = _mapper.Map<TEntity>(resource);
            entity = await _entities.UpdateAsync(id, entity);
            return _mapper.Map<TResource>(entity);
        }

        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, List<DocumentData> relationships)
        {
            var entity = await _entities.GetAndIncludeAsync(id, relationshipName);
            if (entity == null)
            {
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");
            }

            var relationship = _jsonApiContext.ContextGraph
                .GetContextEntity(typeof(TResource))
                .Relationships
                .FirstOrDefault(r => r.PublicRelationshipName == relationshipName);
            var relationshipType = relationship.Type;

            // update relationship type with internalname
            var entityProperty = typeof(TEntity).GetProperty(relationship.InternalRelationshipName);
            if (entityProperty == null)
            {
                throw new JsonApiException(404, $"Property {relationship.InternalRelationshipName} could not be found on entity.");
            }
            relationship.Type = relationship.IsHasMany ? entityProperty.PropertyType.GetGenericArguments()[0] : entityProperty.PropertyType;

            var relationshipIds = relationships.Select(r => r?.Id?.ToString());

            await _entities.UpdateRelationshipsAsync(entity, relationship, relationshipIds);

            relationship.Type = relationshipType;
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            return await _entities.DeleteAsync(id);
        }

        protected virtual IQueryable<TEntity> ApplySortAndFilterQuery(IQueryable<TEntity> entities)
        {
            var query = _jsonApiContext.QuerySet;

            if (_jsonApiContext.QuerySet == null)
                return entities;

            if (query.Filters.Count > 0)
                foreach (var filter in query.Filters)
                    entities = _entities.Filter(entities, filter);

            if (query.SortParameters != null && query.SortParameters.Count > 0)
                entities = _entities.Sort(entities, query.SortParameters);

            return entities;
        }

        protected virtual async Task<IEnumerable<TResource>> ApplyPageQueryAsync(IQueryable<TEntity> entities)
        {
            var pageManager = _jsonApiContext.PageManager;
            if (!pageManager.IsPaginated)
                return _mapper.Map<IEnumerable<TResource>>(await _entities.ToListAsync(entities));

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation($"Applying paging query. Fetching page {pageManager.CurrentPage} with {pageManager.PageSize} entities");
            }

            return _mapper.Map<IEnumerable<TResource>>(await _entities.PageAsync(entities, pageManager.PageSize, pageManager.CurrentPage));
        }

        protected virtual IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, List<string> relationships)
        {
            _jsonApiContext.IncludedRelationships = relationships;

            foreach (var r in relationships)
                entities = _entities.Include(entities, r);

            return entities;
        }
    }
}
