using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.RequestServices;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Entity mapping class
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class DefaultResourceService<TResource, TId> :
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IPageService _pageService;
        private readonly IJsonApiOptions _options;
        private readonly IFilterService _filterService;
        private readonly ISortService _sortService;
        private readonly IResourceRepository<TResource, TId> _repository;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceFactory _resourceFactory;
        private readonly ILogger _logger;
        private readonly IResourceHookExecutor _hookExecutor;
        private readonly IIncludeService _includeService;
        private readonly ISparseFieldsService _sparseFieldsService;
        private readonly ResourceContext _currentRequestResource;

        public DefaultResourceService(
            IEnumerable<IQueryParameterService> queryParameters,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceRepository<TResource, TId> repository,
            IResourceContextProvider provider,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            IResourceHookExecutor hookExecutor = null)
        {
            _includeService = queryParameters.FirstOrDefault<IIncludeService>();
            _sparseFieldsService = queryParameters.FirstOrDefault<ISparseFieldsService>();
            _pageService = queryParameters.FirstOrDefault<IPageService>();
            _sortService = queryParameters.FirstOrDefault<ISortService>();
            _filterService = queryParameters.FirstOrDefault<IFilterService>();
            _options = options;
            _logger = loggerFactory.CreateLogger<DefaultResourceService<TResource, TId>>();
            _repository = repository;
            _resourceChangeTracker = resourceChangeTracker;
            _resourceFactory = resourceFactory;
            _hookExecutor = hookExecutor;
            _currentRequestResource = provider.GetResourceContext<TResource>();
        }

        public virtual async Task<TResource> CreateAsync(TResource entity)
        {
            _logger.LogTrace($"Entering {nameof(CreateAsync)}(object).");
            
            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeCreate(AsList(entity), ResourcePipeline.Post).SingleOrDefault();
            await _repository.CreateAsync(entity);

            entity = await GetWithRelationshipsAsync(entity.Id);

            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterCreate(AsList(entity), ResourcePipeline.Post);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Get).SingleOrDefault();
            }
            return entity;
        }

        public virtual async Task DeleteAsync(TId id)
        {
            _logger.LogTrace($"Entering {nameof(DeleteAsync)}('{id}').");

            if (!IsNull(_hookExecutor))
            {
                var entity = _resourceFactory.CreateInstance<TResource>();
                entity.Id = id;

                _hookExecutor.BeforeDelete(AsList(entity), ResourcePipeline.Delete);
            }

            var succeeded = await _repository.DeleteAsync(id);
            if (!succeeded)
            {
                string resourceId = TypeHelper.GetResourceStringId<TResource, TId>(id, _resourceFactory);
                throw new ResourceNotFoundException(resourceId, _currentRequestResource.ResourceName);
            }

            if (!IsNull(_hookExecutor))
            {
                var entity = _resourceFactory.CreateInstance<TResource>();
                entity.Id = id;

                _hookExecutor.AfterDelete(AsList(entity), ResourcePipeline.Delete, succeeded);
            }
        }
        
        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
            _logger.LogTrace($"Entering {nameof(GetAsync)}().");

            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.Get);

            var entityQuery = _repository.Get();
            entityQuery = ApplyFilter(entityQuery);
            entityQuery = ApplySort(entityQuery);
            entityQuery = ApplyInclude(entityQuery);
            entityQuery = ApplySelect(entityQuery);

            if (!IsNull(_hookExecutor, entityQuery))
            {
                var entities = await _repository.ToListAsync(entityQuery);
                _hookExecutor.AfterRead(entities, ResourcePipeline.Get);
                entityQuery = _hookExecutor.OnReturn(entities, ResourcePipeline.Get).AsQueryable();
            }

            if (_options.IncludeTotalRecordCount)
                _pageService.TotalRecords = await _repository.CountAsync(entityQuery);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entityQuery);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            _logger.LogTrace($"Entering {nameof(GetAsync)}('{id}').");

            var pipeline = ResourcePipeline.GetSingle;
            _hookExecutor?.BeforeRead<TResource>(pipeline, id.ToString());

            var entityQuery = _repository.Get(id);
            entityQuery = ApplyFilter(entityQuery);
            entityQuery = ApplyInclude(entityQuery);
            entityQuery = ApplySelect(entityQuery);

            var entity = await _repository.FirstOrDefaultAsync(entityQuery);

            if (entity == null)
            {
                string resourceId = TypeHelper.GetResourceStringId<TResource, TId>(id, _resourceFactory);
                throw new ResourceNotFoundException(resourceId, _currentRequestResource.ResourceName);
            }

            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterRead(AsList(entity), pipeline);
                entity = _hookExecutor.OnReturn(AsList(entity), pipeline).SingleOrDefault();
            }

            return entity;
        }

        // triggered by GET /articles/1/relationships/{relationshipName}
        public virtual async Task<TResource> GetRelationshipsAsync(TId id, string relationshipName)
        {
            _logger.LogTrace($"Entering {nameof(GetRelationshipsAsync)}('{id}', '{relationshipName}').");

            var relationship = GetRelationship(relationshipName);

            // BeforeRead hook execution
            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.GetRelationship, id.ToString());

            var entityQuery = ApplyInclude(_repository.Get(id), relationship);
            var entity = await _repository.FirstOrDefaultAsync(entityQuery);

            if (entity == null)
            {
                string resourceId = TypeHelper.GetResourceStringId<TResource, TId>(id, _resourceFactory);
                throw new ResourceNotFoundException(resourceId, _currentRequestResource.ResourceName);
            }

            if (!IsNull(_hookExecutor, entity))
            {   // AfterRead and OnReturn resource hook execution.
                _hookExecutor.AfterRead(AsList(entity), ResourcePipeline.GetRelationship);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.GetRelationship).SingleOrDefault();
            }

            return entity;
        }

        // triggered by GET /articles/1/{relationshipName}
        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            _logger.LogTrace($"Entering {nameof(GetRelationshipAsync)}('{id}', '{relationshipName}').");

            var relationship = GetRelationship(relationshipName);
            var resource = await GetRelationshipsAsync(id, relationshipName);
            return relationship.GetValue(resource);
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource requestEntity)
        {
            _logger.LogTrace($"Entering {nameof(UpdateAsync)}('{id}', {(requestEntity == null ? "null" : "object")}).");

            TResource databaseEntity = await _repository.Get(id).FirstOrDefaultAsync();
            if (databaseEntity == null)
            {
                string resourceId = TypeHelper.GetResourceStringId<TResource, TId>(id, _resourceFactory);
                throw new ResourceNotFoundException(resourceId, _currentRequestResource.ResourceName);
            }

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(databaseEntity);
            _resourceChangeTracker.SetRequestedAttributeValues(requestEntity);

            requestEntity = IsNull(_hookExecutor) ? requestEntity : _hookExecutor.BeforeUpdate(AsList(requestEntity), ResourcePipeline.Patch).Single();

            await _repository.UpdateAsync(requestEntity, databaseEntity);

            if (!IsNull(_hookExecutor, databaseEntity))
            {
                _hookExecutor.AfterUpdate(AsList(databaseEntity), ResourcePipeline.Patch);
                _hookExecutor.OnReturn(AsList(databaseEntity), ResourcePipeline.Patch);
            }

            _repository.FlushFromCache(databaseEntity);
            TResource afterEntity = await _repository.Get(id).FirstOrDefaultAsync();
            _resourceChangeTracker.SetFinallyStoredAttributeValues(afterEntity);

            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
            return hasImplicitChanges ? afterEntity : null;
        }

        // triggered by PATCH /articles/1/relationships/{relationshipName}
        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, object related)
        {
            _logger.LogTrace($"Entering {nameof(UpdateRelationshipsAsync)}('{id}', '{relationshipName}', {(related == null ? "null" : "object")}).");

            var relationship = GetRelationship(relationshipName);
            var entityQuery = _repository.Include(_repository.Get(id), new[] { relationship });
            var entity = await _repository.FirstOrDefaultAsync(entityQuery);

            if (entity == null)
            {
                string resourceId = TypeHelper.GetResourceStringId<TResource, TId>(id, _resourceFactory);
                throw new ResourceNotFoundException(resourceId, _currentRequestResource.ResourceName);
            }

            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.PatchRelationship).SingleOrDefault();

            string[] relationshipIds = null;
            if (related != null)
            {
                relationshipIds = relationship is HasOneAttribute
                    ? new[] {((IIdentifiable) related).StringId}
                    : ((IEnumerable<IIdentifiable>) related).Select(e => e.StringId).ToArray();
            }

            await _repository.UpdateRelationshipsAsync(entity, relationship, relationshipIds ?? Array.Empty<string>());

            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.PatchRelationship);
        }

        protected virtual async Task<IEnumerable<TResource>> ApplyPageQueryAsync(IQueryable<TResource> entities)
        {
            if (_pageService.PageSize == 0)
            {
                _logger.LogDebug("Fetching complete result set.");

                return await _repository.ToListAsync(entities);
            }

            int pageOffset = _pageService.CurrentPage;
            if (_pageService.Backwards)
            {
                pageOffset = -pageOffset;
            }

            _logger.LogDebug($"Fetching paged result set at page {pageOffset} with size {_pageService.PageSize}.");

            return await _repository.PageAsync(entities, _pageService.PageSize, pageOffset);
        }

        /// <summary>
        /// Applies sort queries
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual IQueryable<TResource> ApplySort(IQueryable<TResource> entities)
        {
            var queries = _sortService.Get();
            entities = _repository.Sort(entities, queries);
            return entities;
        }

        /// <summary>
        /// Applies filter queries
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual IQueryable<TResource> ApplyFilter(IQueryable<TResource> entities)
        {
            var queries = _filterService.Get();
            if (queries != null && queries.Any())
                foreach (var query in queries)
                    entities = _repository.Filter(entities, query);

            return entities;
        }

        /// <summary>
        /// Applies include queries
        /// </summary>
        protected virtual IQueryable<TResource> ApplyInclude(IQueryable<TResource> entities, RelationshipAttribute chainPrefix = null)
        {
            var chains = _includeService.Get();

            if (chainPrefix != null)
            {
                chains.Add(new List<RelationshipAttribute>());
            }

            foreach (var inclusionChain in chains)
            {
                if (chainPrefix != null)
                {
                    inclusionChain.Insert(0, chainPrefix);
                }

                entities = _repository.Include(entities, inclusionChain);
            }

            return entities;
        }

        /// <summary>
        /// Applies sparse field selection to queries
        /// </summary>
        protected virtual IQueryable<TResource> ApplySelect(IQueryable<TResource> entities)
        {
            var propertyNames = _sparseFieldsService.GetAll();

            if (propertyNames.Any())
            {
                // All resources without a sparse fieldset specified must be entirely selected.
                EnsureResourcesWithoutSparseFieldSetAreAddedToSelect(propertyNames);
            }

            entities = _repository.Select(entities, propertyNames);
            return entities;
        }

        private void EnsureResourcesWithoutSparseFieldSetAreAddedToSelect(ISet<string> propertyNames)
        {
            bool hasTopLevelSparseFieldSet = propertyNames.Any(x => !x.Contains("."));
            if (!hasTopLevelSparseFieldSet)
            {
                var topPropertyNames = _currentRequestResource.Attributes
                    .Where(x => x.Property.SetMethod != null)
                    .Select(x => x.Property.Name);
                propertyNames.AddRange(topPropertyNames);
            }

            var chains = _includeService.Get();
            foreach (var inclusionChain in chains)
            {
                string relationshipPath = null;
                foreach (var relationship in inclusionChain)
                {
                    relationshipPath = relationshipPath == null
                        ? relationship.RelationshipPath
                        : $"{relationshipPath}.{relationship.RelationshipPath}";
                }

                if (relationshipPath != null)
                {
                    bool hasRelationSparseFieldSet = propertyNames.Any(x => x.StartsWith(relationshipPath + "."));
                    if (!hasRelationSparseFieldSet)
                    {
                        propertyNames.Add(relationshipPath);
                    }
                }
            }
        }

        /// <summary>
        /// Get the specified id with relationships provided in the post request
        /// </summary>
        private async Task<TResource> GetWithRelationshipsAsync(TId id)
        {
            var query = _repository.Get(id);
            query = ApplyInclude(query);
            query = ApplySelect(query);

            var entity = await _repository.FirstOrDefaultAsync(query);
            return entity;
        }

        private bool IsNull(params object[] values)
        {
            foreach (var val in values)
            {
                if (val == null) return true;
            }
            return false;
        }

        private RelationshipAttribute GetRelationship(string relationshipName)
        {
            var relationship = _currentRequestResource.Relationships.SingleOrDefault(r => relationshipName == r.PublicName);
            if (relationship == null)
            {
                throw new RelationshipNotFoundException(relationshipName, _currentRequestResource.ResourceName);
            }
            return relationship;
        }

        private List<TResource> AsList(TResource entity)
        {
            return new List<TResource> { entity };
        }
    }

    /// <summary>
    /// No mapping with integer as default
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class DefaultResourceService<TResource> : DefaultResourceService<TResource, int>,
        IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public DefaultResourceService(
            IEnumerable<IQueryParameterService> queryParameters,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceRepository<TResource, int> repository,
            IResourceContextProvider provider,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            IResourceHookExecutor hookExecutor = null)
            : base(queryParameters, options, loggerFactory, repository, provider, resourceChangeTracker, resourceFactory, hookExecutor)
        { }
    }
}
