using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Services
{
    /// <inheritdoc />
    [PublicAPI]
    public class JsonApiResourceService<TResource, TId> : IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly CollectionConverter _collectionConverter = new CollectionConverter();
        private readonly IResourceRepositoryAccessor _repositoryAccessor;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly TraceLogWriter<JsonApiResourceService<TResource, TId>> _traceWriter;
        private readonly IJsonApiRequest _request;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceHookExecutorFacade _hookExecutor;

        public JsonApiResourceService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker, IResourceHookExecutorFacade hookExecutor)
        {
            ArgumentGuard.NotNull(repositoryAccessor, nameof(repositoryAccessor));
            ArgumentGuard.NotNull(queryLayerComposer, nameof(queryLayerComposer));
            ArgumentGuard.NotNull(paginationContext, nameof(paginationContext));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(resourceChangeTracker, nameof(resourceChangeTracker));
            ArgumentGuard.NotNull(hookExecutor, nameof(hookExecutor));

            _repositoryAccessor = repositoryAccessor;
            _queryLayerComposer = queryLayerComposer;
            _paginationContext = paginationContext;
            _options = options;
            _request = request;
            _resourceChangeTracker = resourceChangeTracker;
            _hookExecutor = hookExecutor;
            _traceWriter = new TraceLogWriter<JsonApiResourceService<TResource, TId>>(loggerFactory);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart();

            _hookExecutor.BeforeReadMany<TResource>();

            if (_options.IncludeTotalResourceCount)
            {
                FilterExpression topFilter = _queryLayerComposer.GetTopFilterFromConstraints(_request.PrimaryResource);
                _paginationContext.TotalResourceCount = await _repositoryAccessor.CountAsync<TResource>(topFilter, cancellationToken);

                if (_paginationContext.TotalResourceCount == 0)
                {
                    return Array.Empty<TResource>();
                }
            }

            QueryLayer queryLayer = _queryLayerComposer.ComposeFromConstraints(_request.PrimaryResource);
            IReadOnlyCollection<TResource> resources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

            if (queryLayer.Pagination?.PageSize != null && queryLayer.Pagination.PageSize.Value == resources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            _hookExecutor.AfterReadMany(resources);
            return _hookExecutor.OnReturnMany(resources);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> GetAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetSingle);

            TResource primaryResource = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.PreserveExisting, cancellationToken);
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetSingle);
            _hookExecutor.OnReturnSingle(primaryResource, ResourcePipeline.GetSingle);

            return primaryResource;
        }

        /// <inheritdoc />
        public virtual async Task<object> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                relationshipName
            });

            AssertHasRelationship(_request.Relationship, relationshipName);

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetRelationship);

            QueryLayer secondaryLayer = _queryLayerComposer.ComposeFromConstraints(_request.SecondaryResource);
            QueryLayer primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResource, id, _request.Relationship);

            if (_request.IsCollection && _options.IncludeTotalResourceCount)
            {
                // TODO: Consider support for pagination links on secondary resource collection. This requires to call Count() on the inverse relationship (which may not exist).
                // For /blogs/1/articles we need to execute Count(Articles.Where(article => article.Blog.Id == 1 && article.Blog.existingFilter))) to determine TotalResourceCount.
                // This also means we need to invoke ResourceRepository<Article>.CountAsync() from ResourceService<Blog>.
                // And we should call BlogResourceDefinition.OnApplyFilter to filter out soft-deleted blogs and translate from equals('IsDeleted','false') to equals('Blog.IsDeleted','false')
            }

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

            TResource primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetRelationship);

            object secondaryResourceOrResources = _request.Relationship.GetValue(primaryResource);

            if (secondaryResourceOrResources is ICollection secondaryResources && secondaryLayer.Pagination?.PageSize?.Value == secondaryResources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            return _hookExecutor.OnReturnRelationship(secondaryResourceOrResources);
        }

        /// <inheritdoc />
        public virtual async Task<object> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                relationshipName
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));

            AssertHasRelationship(_request.Relationship, relationshipName);

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetRelationship);

            QueryLayer secondaryLayer = _queryLayerComposer.ComposeSecondaryLayerForRelationship(_request.SecondaryResource);
            QueryLayer primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResource, id, _request.Relationship);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

            TResource primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetRelationship);

            object secondaryResourceOrResources = _request.Relationship.GetValue(primaryResource);

            return _hookExecutor.OnReturnRelationship(secondaryResourceOrResources);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                resource
            });

            ArgumentGuard.NotNull(resource, nameof(resource));

            TResource resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestedAttributeValues(resourceFromRequest);

            _hookExecutor.BeforeCreate(resourceFromRequest);

            TResource resourceForDatabase = await _repositoryAccessor.GetForCreateAsync<TResource, TId>(resource.Id, cancellationToken);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceForDatabase);

            try
            {
                await _repositoryAccessor.CreateAsync(resourceFromRequest, resourceForDatabase, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                if (!Equals(resourceFromRequest.Id, default(TId)))
                {
                    TResource existingResource =
                        await TryGetPrimaryResourceByIdAsync(resourceFromRequest.Id, TopFieldSelection.OnlyIdAttribute, cancellationToken);

                    if (existingResource != null)
                    {
                        throw new ResourceAlreadyExistsException(resourceFromRequest.StringId, _request.PrimaryResource.PublicName);
                    }
                }

                await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest, cancellationToken);
                throw;
            }

            TResource resourceFromDatabase =
                await TryGetPrimaryResourceByIdAsync(resourceForDatabase.Id, TopFieldSelection.WithAllAttributes, cancellationToken);

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

        private async Task AssertResourcesToAssignInRelationshipsExistAsync(TResource resource, CancellationToken cancellationToken)
        {
            var missingResources = new List<MissingResourceInRelationship>();

            foreach ((QueryLayer queryLayer, RelationshipAttribute relationship) in _queryLayerComposer.ComposeForGetTargetedSecondaryResourceIds(resource))
            {
                object rightValue = relationship.GetValue(resource);
                ICollection<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue);

                IAsyncEnumerable<MissingResourceInRelationship> missingResourcesInRelationship =
                    GetMissingRightResourcesAsync(queryLayer, relationship, rightResourceIds, cancellationToken);

                await missingResources.AddRangeAsync(missingResourcesInRelationship, cancellationToken);
            }

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingRightResourcesAsync(QueryLayer existingRightResourceIdsQueryLayer,
            RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            IReadOnlyCollection<IIdentifiable> existingResources = await _repositoryAccessor.GetAsync(
                existingRightResourceIdsQueryLayer.ResourceContext.ResourceType, existingRightResourceIdsQueryLayer, cancellationToken);

            string[] existingResourceIds = existingResources.Select(resource => resource.StringId).ToArray();

            foreach (IIdentifiable rightResourceId in rightResourceIds)
            {
                if (!existingResourceIds.Contains(rightResourceId.StringId))
                {
                    yield return new MissingResourceInRelationship(relationship.PublicName, existingRightResourceIdsQueryLayer.ResourceContext.PublicName,
                        rightResourceId.StringId);
                }
            }
        }

        /// <inheritdoc />
        public async Task AddToToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                primaryId,
                secondaryResourceIds
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));
            ArgumentGuard.NotNull(secondaryResourceIds, nameof(secondaryResourceIds));

            AssertHasRelationship(_request.Relationship, relationshipName);

            if (secondaryResourceIds.Any())
            {
                if (_request.Relationship is HasManyThroughAttribute hasManyThrough)
                {
                    // In the case of a many-to-many relationship, creating a duplicate entry in the join table results in a
                    // unique constraint violation. We avoid that by excluding already-existing entries from the set in advance.
                    await RemoveExistingIdsFromSecondarySetAsync(primaryId, secondaryResourceIds, hasManyThrough, cancellationToken);
                }

                try
                {
                    await _repositoryAccessor.AddToToManyRelationshipAsync<TResource, TId>(primaryId, secondaryResourceIds, cancellationToken);
                }
                catch (DataStoreUpdateException)
                {
                    TResource primaryResource = await TryGetPrimaryResourceByIdAsync(primaryId, TopFieldSelection.OnlyIdAttribute, cancellationToken);
                    AssertPrimaryResourceExists(primaryResource);

                    await AssertResourcesExistAsync(secondaryResourceIds, cancellationToken);
                    throw;
                }
            }
        }

        private async Task RemoveExistingIdsFromSecondarySetAsync(TId primaryId, ISet<IIdentifiable> secondaryResourceIds,
            HasManyThroughAttribute hasManyThrough, CancellationToken cancellationToken)
        {
            QueryLayer queryLayer = _queryLayerComposer.ComposeForHasMany(hasManyThrough, primaryId, secondaryResourceIds);
            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

            TResource primaryResource = primaryResources.FirstOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            object rightValue = _request.Relationship.GetValue(primaryResource);
            ICollection<IIdentifiable> existingRightResourceIds = _collectionConverter.ExtractResources(rightValue);

            secondaryResourceIds.ExceptWith(existingRightResourceIds);
        }

        private async Task AssertResourcesExistAsync(ICollection<IIdentifiable> secondaryResourceIds, CancellationToken cancellationToken)
        {
            QueryLayer queryLayer = _queryLayerComposer.ComposeForGetRelationshipRightIds(_request.Relationship, secondaryResourceIds);

            List<MissingResourceInRelationship> missingResources =
                await GetMissingRightResourcesAsync(queryLayer, _request.Relationship, secondaryResourceIds, cancellationToken).ToListAsync(cancellationToken);

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        /// <inheritdoc />
        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                resource
            });

            ArgumentGuard.NotNull(resource, nameof(resource));

            TResource resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestedAttributeValues(resourceFromRequest);

            _hookExecutor.BeforeUpdateResource(resourceFromRequest);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id, cancellationToken);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceFromDatabase);

            try
            {
                await _repositoryAccessor.UpdateAsync(resourceFromRequest, resourceFromDatabase, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest, cancellationToken);
                throw;
            }

            TResource afterResourceFromDatabase = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.WithAllAttributes, cancellationToken);
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
        public virtual async Task SetRelationshipAsync(TId primaryId, string relationshipName, object secondaryResourceIds, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                primaryId,
                relationshipName,
                secondaryResourceIds
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));

            AssertHasRelationship(_request.Relationship, relationshipName);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(primaryId, cancellationToken);

            _hookExecutor.BeforeUpdateRelationship(resourceFromDatabase);

            try
            {
                await _repositoryAccessor.SetRelationshipAsync(resourceFromDatabase, secondaryResourceIds, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                await AssertResourcesExistAsync(_collectionConverter.ExtractResources(secondaryResourceIds), cancellationToken);
                throw;
            }

            _hookExecutor.AfterUpdateRelationship(resourceFromDatabase);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            _hookExecutor.BeforeDelete<TResource, TId>(id);

            try
            {
                await _repositoryAccessor.DeleteAsync<TResource, TId>(id, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                TResource primaryResource = await TryGetPrimaryResourceByIdAsync(id, TopFieldSelection.OnlyIdAttribute, cancellationToken);
                AssertPrimaryResourceExists(primaryResource);
                throw;
            }

            _hookExecutor.AfterDelete<TResource, TId>(id);
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync(TId primaryId, string relationshipName, ISet<IIdentifiable> secondaryResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                primaryId,
                relationshipName,
                secondaryResourceIds
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));
            ArgumentGuard.NotNull(secondaryResourceIds, nameof(secondaryResourceIds));

            AssertHasRelationship(_request.Relationship, relationshipName);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(primaryId, cancellationToken);
            await AssertResourcesExistAsync(secondaryResourceIds, cancellationToken);

            if (secondaryResourceIds.Any())
            {
                await _repositoryAccessor.RemoveFromToManyRelationshipAsync(resourceFromDatabase, secondaryResourceIds, cancellationToken);
            }
        }

        private async Task<TResource> TryGetPrimaryResourceByIdAsync(TId id, TopFieldSelection fieldSelection, CancellationToken cancellationToken)
        {
            QueryLayer primaryLayer = _queryLayerComposer.ComposeForGetById(id, _request.PrimaryResource, fieldSelection);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);
            return primaryResources.SingleOrDefault();
        }

        private async Task<TResource> GetPrimaryResourceForUpdateAsync(TId id, CancellationToken cancellationToken)
        {
            QueryLayer queryLayer = _queryLayerComposer.ComposeForUpdate(id, _request.PrimaryResource);
            var resource = await _repositoryAccessor.GetForUpdateAsync<TResource>(queryLayer, cancellationToken);

            AssertPrimaryResourceExists(resource);
            return resource;
        }

        [AssertionMethod]
        private void AssertPrimaryResourceExists(TResource resource)
        {
            if (resource == null)
            {
                throw new ResourceNotFoundException(_request.PrimaryId, _request.PrimaryResource.PublicName);
            }
        }

        [AssertionMethod]
        private void AssertHasRelationship(RelationshipAttribute relationship, string name)
        {
            if (relationship == null)
            {
                throw new RelationshipNotFoundException(name, _request.PrimaryResource.PublicName);
            }
        }
    }

    /// <summary>
    /// Represents the foundational Resource Service layer in the JsonApiDotNetCore architecture that uses a Resource Repository for data access.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    [PublicAPI]
    public class JsonApiResourceService<TResource> : JsonApiResourceService<TResource, int>, IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public JsonApiResourceService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker, IResourceHookExecutorFacade hookExecutor)
            : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker, hookExecutor)
        {
        }
    }
}
