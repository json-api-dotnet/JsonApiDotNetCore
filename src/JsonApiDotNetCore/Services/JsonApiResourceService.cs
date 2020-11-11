using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    public class JsonApiResourceService<TResource, TId> :
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IResourceRepository<TResource, TId> _repository;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly TraceLogWriter<JsonApiResourceService<TResource, TId>> _traceWriter;
        private readonly IJsonApiRequest _request;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceFactory _resourceFactory;
        private readonly ISecondaryResourceResolver _secondaryResourceResolver;
        private readonly IResourceHookExecutorFacade _hookExecutor;

        public JsonApiResourceService(
            IResourceRepository<TResource, TId> repository,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            ISecondaryResourceResolver secondaryResourceResolver,
            IResourceHookExecutorFacade hookExecutor)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _queryLayerComposer = queryLayerComposer ?? throw new ArgumentNullException(nameof(queryLayerComposer));
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _traceWriter = new TraceLogWriter<JsonApiResourceService<TResource, TId>>(loggerFactory);
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _resourceChangeTracker = resourceChangeTracker ?? throw new ArgumentNullException(nameof(resourceChangeTracker));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _secondaryResourceResolver = secondaryResourceResolver ?? throw new ArgumentNullException(nameof(secondaryResourceResolver));
            _hookExecutor = hookExecutor ?? throw new ArgumentNullException(nameof(hookExecutor));
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync()
        {
            _traceWriter.LogMethodStart();

            _hookExecutor.BeforeReadMany<TResource>();

            if (_options.IncludeTotalResourceCount)
            {
                var topFilter = _queryLayerComposer.GetTopFilterFromConstraints();
                _paginationContext.TotalResourceCount = await _repository.CountAsync(topFilter);

                if (_paginationContext.TotalResourceCount == 0)
                {
                    return Array.Empty<TResource>();
                }
            }

            var queryLayer = _queryLayerComposer.ComposeFromConstraints(_request.PrimaryResource);
            var resources = await _repository.GetAsync(queryLayer);

            if (queryLayer.Pagination?.PageSize != null && queryLayer.Pagination.PageSize.Value == resources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            _hookExecutor.AfterReadMany(resources);
            return _hookExecutor.OnReturnMany(resources);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> GetAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetSingle);

            var primaryResource = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.PreserveExisting);
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetSingle);
            _hookExecutor.OnReturnSingle(primaryResource, ResourcePipeline.GetSingle);

            return primaryResource;
        }

        /// <inheritdoc />
        public virtual async Task<object> GetSecondaryAsync(TId id, string relationshipName)
        {
            _traceWriter.LogMethodStart(new {id, relationshipName});
            AssertHasRelationship(_request.Relationship, relationshipName);

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetRelationship);

            var secondaryLayer = _queryLayerComposer.ComposeFromConstraints(_request.SecondaryResource);
            var primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResource, id, _request.Relationship);

            if (_request.IsCollection && _options.IncludeTotalResourceCount)
            {
                // TODO: Consider support for pagination links on secondary resource collection. This requires to call Count() on the inverse relationship (which may not exist).
                // For /blogs/1/articles we need to execute Count(Articles.Where(article => article.Blog.Id == 1 && article.Blog.existingFilter))) to determine TotalResourceCount.
                // This also means we need to invoke ResourceRepository<Article>.CountAsync() from ResourceService<Blog>.
                // And we should call BlogResourceDefinition.OnApplyFilter to filter out soft-deleted blogs and translate from equals('IsDeleted','false') to equals('Blog.IsDeleted','false')
            }

            var primaryResources = await _repository.GetAsync(primaryLayer);

            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetRelationship);

            var secondaryResourceOrResources = _request.Relationship.GetValue(primaryResource);

            if (secondaryResourceOrResources is ICollection secondaryResources &&
                secondaryLayer.Pagination?.PageSize?.Value == secondaryResources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            return _hookExecutor.OnReturnRelationship(secondaryResourceOrResources);
        }

        /// <inheritdoc />
        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName)
        {
            _traceWriter.LogMethodStart(new {id, relationshipName});
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));

            AssertHasRelationship(_request.Relationship, relationshipName);

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetRelationship);

            var secondaryLayer = _queryLayerComposer.ComposeSecondaryLayerForRelationship(_request.SecondaryResource);
            var primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResource, id, _request.Relationship);

            var primaryResources = await _repository.GetAsync(primaryLayer);

            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetRelationship);

            var secondaryResourceOrResources = _request.Relationship.GetValue(primaryResource);

            return _hookExecutor.OnReturnRelationship(secondaryResourceOrResources);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            _traceWriter.LogMethodStart(new {resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            _resourceChangeTracker.SetRequestedAttributeValues(resource);

            var defaultResource = _resourceFactory.CreateInstance<TResource>();
            defaultResource.Id = resource.Id;

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(defaultResource);

            _hookExecutor.BeforeCreate(resource);

            try
            {
                await _repository.CreateAsync(resource);
            }
            catch (DataStoreUpdateException)
            {
                var existingResource = await TryGetPrimaryResourceByIdAsync(resource.Id, TopFieldSelection.OnlyIdAttribute);
                if (existingResource != null)
                {
                    throw new ResourceAlreadyExistsException(resource.StringId, _request.PrimaryResource.PublicName);
                }

                await AssertResourcesToAssignInRelationshipsExistAsync(resource);
                throw;
            }

            var resourceFromDatabase = await TryGetPrimaryResourceByIdAsync(resource.Id, TopFieldSelection.WithAllAttributes);
            AssertPrimaryResourceExists(resourceFromDatabase);

            _hookExecutor.AfterCreate(resourceFromDatabase);

            _resourceChangeTracker.SetFinallyStoredAttributeValues(resourceFromDatabase);

            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
            if (!hasImplicitChanges)
            {
                return null;
            }

            _hookExecutor.OnReturnSingle(resourceFromDatabase, ResourcePipeline.Post);
            return resourceFromDatabase;
        }

        private async Task AssertResourcesToAssignInRelationshipsExistAsync(TResource resource)
        {
            var missingResources = await _secondaryResourceResolver.GetMissingResourcesToAssignInRelationships(resource);
            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        /// <inheritdoc />
        public async Task AddToToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {primaryId, secondaryResourceIds});
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            AssertHasRelationship(_request.Relationship, relationshipName);
            AssertRelationshipIsToMany(_request.Relationship);

            if (secondaryResourceIds.Any())
            {
                var joinTableFilter = _request.Relationship is HasManyThroughAttribute hasManyThrough
                    ? _queryLayerComposer.GetJoinTableFilter(primaryId,
                        secondaryResourceIds.Select(x => x.GetTypedId()).ToArray(), hasManyThrough)
                    : null;

                try
                {
                    await _repository.AddToToManyRelationshipAsync(primaryId, secondaryResourceIds, joinTableFilter);
                }
                catch (DataStoreUpdateException)
                {
                    var primaryResource = await TryGetPrimaryResourceByIdAsync(primaryId, TopFieldSelection.OnlyIdAttribute);
                    AssertPrimaryResourceExists(primaryResource);

                    await AssertResourcesExistAsync(secondaryResourceIds);
                    throw;
                }
            }
        }

        private async Task AssertResourcesExistAsync(ICollection<IIdentifiable> secondaryResourceIds)
        {
            var missingResources = await _secondaryResourceResolver.GetMissingSecondaryResources(_request.Relationship, secondaryResourceIds);
            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        /// <inheritdoc />
        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource)
        {
            _traceWriter.LogMethodStart(new {id, resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestedAttributeValues(resourceFromRequest);

            _hookExecutor.BeforeUpdateResource(resourceFromRequest);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceFromDatabase);

            try
            {
                await _repository.UpdateAsync(resourceFromRequest, resourceFromDatabase);
            }
            catch (DataStoreUpdateException)
            {
                await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest);
                throw;
            }

            TResource afterResourceFromDatabase = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.WithAllAttributes);
            AssertPrimaryResourceExists(afterResourceFromDatabase);

            _hookExecutor.AfterUpdateResource(afterResourceFromDatabase);

            _resourceChangeTracker.SetFinallyStoredAttributeValues(afterResourceFromDatabase);

            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
            if (!hasImplicitChanges)
            {
                return null;
            }

            _hookExecutor.OnReturnSingle(afterResourceFromDatabase, ResourcePipeline.Patch);
            return afterResourceFromDatabase;
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TId primaryId, string relationshipName, object secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {primaryId, relationshipName, secondaryResourceIds});
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));

            AssertHasRelationship(_request.Relationship, relationshipName);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(primaryId);

            _hookExecutor.BeforeUpdateRelationshipAsync(resourceFromDatabase);

            try
            {
                await _repository.SetRelationshipAsync(resourceFromDatabase, secondaryResourceIds);
            }
            catch (DataStoreUpdateException)
            {
                await AssertResourcesExistAsync(TypeHelper.ExtractResources(secondaryResourceIds));
                throw;
            }

            _hookExecutor.AfterUpdateRelationshipAsync(resourceFromDatabase);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            TResource resourceForHooksCached = null;
            await _hookExecutor.BeforeDeleteAsync(id, async () =>
                resourceForHooksCached = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.WithAllAttributes));

            try
            {
                await _repository.DeleteAsync(id);
            }
            catch (DataStoreUpdateException)
            {
                var primaryResource = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.OnlyIdAttribute);
                AssertPrimaryResourceExists(primaryResource);
                throw;
            }

            await _hookExecutor.AfterDeleteAsync(id, () => Task.FromResult(resourceForHooksCached));
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {primaryId, relationshipName, secondaryResourceIds});
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            AssertHasRelationship(_request.Relationship, relationshipName);
            AssertRelationshipIsToMany(_request.Relationship);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(primaryId);
            await AssertResourcesExistAsync(secondaryResourceIds);

            if (secondaryResourceIds.Any())
            {
                await _repository.RemoveFromToManyRelationshipAsync(resourceFromDatabase, secondaryResourceIds);
            }
        }

        private async Task<TResource> TryGetPrimaryResourceByIdAsync(TId id, TopFieldSelection fieldSelection)
        {
            var primaryLayer = _queryLayerComposer.ComposeForGetById(id, _request.PrimaryResource, fieldSelection);

            var primaryResources = await _repository.GetAsync(primaryLayer);
            return primaryResources.SingleOrDefault();
        }

        private async Task<TResource> GetPrimaryResourceForUpdateAsync(TId id)
        {
            var queryLayer = _queryLayerComposer.ComposeForUpdate(id, _request.PrimaryResource);
            var resource = await _repository.GetForUpdateAsync(queryLayer);

            AssertPrimaryResourceExists(resource);
            return resource;
        }

        private void AssertPrimaryResourceExists(TResource resource)
        {
            if (resource == null)
            {
                throw new ResourceNotFoundException(_request.PrimaryId, _request.PrimaryResource.PublicName);
            }
        }

        private void AssertHasRelationship(RelationshipAttribute relationship, string name)
        {
            if (relationship == null)
            {
                throw new RelationshipNotFoundException(name, _request.PrimaryResource.PublicName);
            }
        }

        private void AssertRelationshipIsToMany(RelationshipAttribute relationship)
        {
            if (!(relationship is HasManyAttribute))
            {
                throw new ToManyRelationshipRequiredException(relationship.PublicName);
            }
        }
    }

    /// <summary>
    /// Represents the foundational Resource Service layer in the JsonApiDotNetCore architecture that uses a Resource Repository for data access.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class JsonApiResourceService<TResource> : JsonApiResourceService<TResource, int>,
        IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public JsonApiResourceService(
            IResourceRepository<TResource> repository,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            ISecondaryResourceResolver secondaryResourceResolver,
            IResourceHookExecutorFacade hookExecutor)
            : base(repository, queryLayerComposer, paginationContext, options, loggerFactory, request,
                resourceChangeTracker, resourceFactory, secondaryResourceResolver, hookExecutor)
        {
        }
    }
}
