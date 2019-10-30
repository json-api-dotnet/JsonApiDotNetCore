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
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Extensions;

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
        private readonly IPageService _pageManager;
        private readonly IJsonApiOptions _options;
        private readonly IFilterService _filterService;
        private readonly ISortService _sortService;
        private readonly IResourceRepository<TResource, TId> _repository;
        private readonly ILogger _logger;
        private readonly IResourceHookExecutor _hookExecutor;
        private readonly IIncludeService _includeService;
        private readonly ISparseFieldsService _sparseFieldsService;
        private readonly ResourceContext _currentRequestResource;

        public DefaultResourceService(
                IEnumerable<IQueryParameterService> queryParameters,
                IJsonApiOptions options,
                IResourceRepository<TResource, TId> repository,
                IResourceContextProvider provider,
                IResourceHookExecutor hookExecutor = null,
                ILoggerFactory loggerFactory = null)
        {
            _includeService = queryParameters.FirstOrDefault<IIncludeService>();
            _sparseFieldsService = queryParameters.FirstOrDefault<ISparseFieldsService>();
            _pageManager = queryParameters.FirstOrDefault<IPageService>();
            _sortService = queryParameters.FirstOrDefault<ISortService>();
            _filterService = queryParameters.FirstOrDefault<IFilterService>();
            _options = options;
            _repository = repository;
            _hookExecutor = hookExecutor;
            _logger = loggerFactory?.CreateLogger<DefaultResourceService<TResource, TId>>();
            _currentRequestResource = provider.GetResourceContext<TResource>();
        }


        public virtual async Task<TResource> CreateAsync(TResource entity)
        {
            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeCreate(AsList(entity), ResourcePipeline.Post).SingleOrDefault();
            entity = await _repository.CreateAsync(entity);

            if (_includeService.Get().Any())
                entity = await GetWithRelationshipsAsync(entity.Id);

            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterCreate(AsList(entity), ResourcePipeline.Post);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Get).SingleOrDefault();
            }
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(TId id)
        {
            var entity = (TResource)Activator.CreateInstance(typeof(TResource));
            entity.Id = id;
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.BeforeDelete(AsList(entity), ResourcePipeline.Delete);
            var succeeded = await _repository.DeleteAsync(entity.Id);
            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterDelete(AsList(entity), ResourcePipeline.Delete, succeeded);
            return succeeded;
        }

        public virtual async Task<IEnumerable<TResource>> GetAsync()
        {
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
                _pageManager.TotalRecords = await _repository.CountAsync(entityQuery);

            // pagination should be done last since it will execute the query
            var pagedEntities = await ApplyPageQueryAsync(entityQuery);
            return pagedEntities;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            var pipeline = ResourcePipeline.GetSingle;
            _hookExecutor?.BeforeRead<TResource>(pipeline, id.ToString());

            var entityQuery = _repository.Get(id);
            entityQuery = ApplyInclude(entityQuery);
            entityQuery = ApplySelect(entityQuery);
            var entity = await _repository.FirstOrDefaultAsync(entityQuery);

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
            var relationship = GetRelationship(relationshipName);

            // BeforeRead hook execution
            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.GetRelationship, id.ToString());

            // TODO: it would be better if we could distinguish whether or not the relationship was not found,
            // vs the relationship not being set on the instance of T
            var entityQuery = _repository.Include(_repository.Get(id), new RelationshipAttribute[] { relationship });
            var entity = await _repository.FirstOrDefaultAsync(entityQuery);
            if (entity == null) // this does not make sense. If the parent entity is not found, this error is thrown?
                throw new JsonApiException(404, $"Relationship '{relationshipName}' not found.");

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
            var relationship = GetRelationship(relationshipName);
            var resource = await GetRelationshipsAsync(id, relationshipName);
            return relationship.GetValue(resource);
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource entity)
        {
            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.Patch).SingleOrDefault();
            entity = await _repository.UpdateAsync(entity);
            if (!IsNull(_hookExecutor, entity))
            {
                _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.Patch);
                entity = _hookExecutor.OnReturn(AsList(entity), ResourcePipeline.Patch).SingleOrDefault();
            }
            return entity;
        }

        // triggered by PATCH /articles/1/relationships/{relationshipName}
        public virtual async Task UpdateRelationshipsAsync(TId id, string relationshipName, object related)
        {
            var relationship = GetRelationship(relationshipName);
            var entityQuery = _repository.Include(_repository.Get(id), new RelationshipAttribute[] { relationship });
            var entity = await _repository.FirstOrDefaultAsync(entityQuery);
            if (entity == null)
                throw new JsonApiException(404, $"Entity with id {id} could not be found.");

            entity = IsNull(_hookExecutor) ? entity : _hookExecutor.BeforeUpdate(AsList(entity), ResourcePipeline.PatchRelationship).SingleOrDefault();

            string[] relationshipIds = null;
            if (related != null)
            {
                if (relationship is HasOneAttribute)
                    relationshipIds = new string[] { ((IIdentifiable)related).StringId };
                else
                    relationshipIds = ((IEnumerable<IIdentifiable>)related).Select(e => e.StringId).ToArray();
            }

            await _repository.UpdateRelationshipsAsync(entity, relationship, relationshipIds ?? new string[0] );

            if (!IsNull(_hookExecutor, entity)) _hookExecutor.AfterUpdate(AsList(entity), ResourcePipeline.PatchRelationship);
        }

        protected virtual async Task<IEnumerable<TResource>> ApplyPageQueryAsync(IQueryable<TResource> entities)
        {
            if (!(_pageManager.PageSize > 0))
            {
                var allEntities = await _repository.ToListAsync(entities);
                return allEntities as IEnumerable<TResource>;
            }

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation($"Applying paging query. Fetching page {_pageManager.CurrentPage} " +
                    $"with {_pageManager.PageSize} entities");
            }

            return await _repository.PageAsync(entities, _pageManager.PageSize, _pageManager.CurrentPage);
        }

        /// <summary>
        /// Applies sort queries
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual IQueryable<TResource> ApplySort(IQueryable<TResource> entities)
        {
            var queries = _sortService.Get();
            if (queries != null && queries.Any())
                foreach (var query in queries)
                    entities = _repository.Sort(entities, query);

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
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual IQueryable<TResource> ApplyInclude(IQueryable<TResource> entities)
        {
            var chains = _includeService.Get();
            if (chains != null && chains.Any())
                foreach (var r in chains)
                    entities = _repository.Include(entities, r.ToArray());

            return entities;
        }

        /// <summary>
        /// Applies sparse field selection queries
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual IQueryable<TResource> ApplySelect(IQueryable<TResource> entities)
        {
            var fields = _sparseFieldsService.Get();
            if (fields != null && fields.Any())
                entities = _repository.Select(entities, fields.ToArray());

            return entities;
        }

        /// <summary>
        /// Get the specified id with relationships provided in the post request
        /// </summary>
        /// <param name="id"></param>i
        /// <returns></returns>
        private async Task<TResource> GetWithRelationshipsAsync(TId id)
        {
            var sparseFieldset = _sparseFieldsService.Get();
            var query = _repository.Select(_repository.Get(id), sparseFieldset.ToArray());

            foreach (var chain in _includeService.Get())
                query = _repository.Include(query, chain.ToArray());

            TResource value;
            // https://github.com/aspnet/EntityFrameworkCore/issues/6573
            if (sparseFieldset.Any())
                value = query.FirstOrDefault();
            else
                value = await _repository.FirstOrDefaultAsync(query);


            return value;
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
            var relationship = _currentRequestResource.Relationships.Single(r => r.Is(relationshipName));
            if (relationship == null)
                throw new JsonApiException(422, $"Relationship '{relationshipName}' does not exist on resource '{typeof(TResource)}'.");
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
        public DefaultResourceService(IEnumerable<IQueryParameterService> queryParameters,
                                      IJsonApiOptions options,
                                      IResourceRepository<TResource, int> repository,
                                      IResourceContextProvider provider,
                                      IResourceHookExecutor hookExecutor = null,
                                      ILoggerFactory loggerFactory = null)
            : base(queryParameters, options, repository, provider, hookExecutor, loggerFactory) { }
    }
}
