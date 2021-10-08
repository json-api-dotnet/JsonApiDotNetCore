using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
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
        private readonly CollectionConverter _collectionConverter = new();
        private readonly IResourceRepositoryAccessor _repositoryAccessor;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly TraceLogWriter<JsonApiResourceService<TResource, TId>> _traceWriter;
        private readonly IJsonApiRequest _request;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;

        public JsonApiResourceService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker, IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            ArgumentGuard.NotNull(repositoryAccessor, nameof(repositoryAccessor));
            ArgumentGuard.NotNull(queryLayerComposer, nameof(queryLayerComposer));
            ArgumentGuard.NotNull(paginationContext, nameof(paginationContext));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(resourceChangeTracker, nameof(resourceChangeTracker));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

            _repositoryAccessor = repositoryAccessor;
            _queryLayerComposer = queryLayerComposer;
            _paginationContext = paginationContext;
            _options = options;
            _request = request;
            _resourceChangeTracker = resourceChangeTracker;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _traceWriter = new TraceLogWriter<JsonApiResourceService<TResource, TId>>(loggerFactory);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync(CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart();

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get resources");

            if (_options.IncludeTotalResourceCount)
            {
                FilterExpression topFilter = _queryLayerComposer.GetTopFilterFromConstraints(_request.PrimaryResourceType);
                _paginationContext.TotalResourceCount = await _repositoryAccessor.CountAsync<TResource>(topFilter, cancellationToken);

                if (_paginationContext.TotalResourceCount == 0)
                {
                    return Array.Empty<TResource>();
                }
            }

            QueryLayer queryLayer = _queryLayerComposer.ComposeFromConstraints(_request.PrimaryResourceType);
            IReadOnlyCollection<TResource> resources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

            if (queryLayer.Pagination?.PageSize != null && queryLayer.Pagination.PageSize.Value == resources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            return resources;
        }

        /// <inheritdoc />
        public virtual async Task<TResource> GetAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get single resource");

            return await GetPrimaryResourceByIdAsync(id, TopFieldSelection.PreserveExisting, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<object> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id,
                relationshipName
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get secondary resource(s)");

            AssertHasRelationship(_request.Relationship, relationshipName);

            QueryLayer secondaryLayer = _queryLayerComposer.ComposeFromConstraints(_request.SecondaryResourceType);

            QueryLayer primaryLayer =
                _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResourceType, id, _request.Relationship);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

            TResource primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            object rightValue = _request.Relationship.GetValue(primaryResource);

            if (rightValue is ICollection rightResources && secondaryLayer.Pagination?.PageSize?.Value == rightResources.Count)
            {
                _paginationContext.IsPageFull = true;
            }

            return rightValue;
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

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Get relationship");

            AssertHasRelationship(_request.Relationship, relationshipName);

            QueryLayer secondaryLayer = _queryLayerComposer.ComposeSecondaryLayerForRelationship(_request.SecondaryResourceType);

            QueryLayer primaryLayer =
                _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResourceType, id, _request.Relationship);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);

            TResource primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            return _request.Relationship.GetValue(primaryResource);
        }

        /// <inheritdoc />
        public virtual async Task<TResource> CreateAsync(TResource resource, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                resource
            });

            ArgumentGuard.NotNull(resource, nameof(resource));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Create resource");

            TResource resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestAttributeValues(resourceFromRequest);

            TResource resourceForDatabase = await _repositoryAccessor.GetForCreateAsync<TResource, TId>(resource.Id, cancellationToken);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceForDatabase);

            await InitializeResourceAsync(resourceForDatabase, cancellationToken);

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
                        throw new ResourceAlreadyExistsException(resourceFromRequest.StringId, _request.PrimaryResourceType.PublicName);
                    }
                }

                await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest, cancellationToken);
                throw;
            }

            TResource resourceFromDatabase = await GetPrimaryResourceByIdAsync(resourceForDatabase.Id, TopFieldSelection.WithAllAttributes, cancellationToken);

            _resourceChangeTracker.SetFinallyStoredAttributeValues(resourceFromDatabase);

            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
            return hasImplicitChanges ? resourceFromDatabase : null;
        }

        protected virtual async Task InitializeResourceAsync(TResource resourceForDatabase, CancellationToken cancellationToken)
        {
            await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceForDatabase, WriteOperationKind.CreateResource, cancellationToken);
        }

        protected async Task AssertResourcesToAssignInRelationshipsExistAsync(TResource primaryResource, CancellationToken cancellationToken)
        {
            var missingResources = new List<MissingResourceInRelationship>();

            foreach ((QueryLayer queryLayer, RelationshipAttribute relationship) in _queryLayerComposer.ComposeForGetTargetedSecondaryResourceIds(
                primaryResource))
            {
                object rightValue = relationship.GetValue(primaryResource);
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
            IReadOnlyCollection<IIdentifiable> existingResources = await _repositoryAccessor.GetAsync(existingRightResourceIdsQueryLayer.ResourceType,
                existingRightResourceIdsQueryLayer, cancellationToken);

            string[] existingResourceIds = existingResources.Select(resource => resource.StringId).ToArray();

            foreach (IIdentifiable rightResourceId in rightResourceIds)
            {
                if (!existingResourceIds.Contains(rightResourceId.StringId))
                {
                    yield return new MissingResourceInRelationship(relationship.PublicName, existingRightResourceIdsQueryLayer.ResourceType.PublicName,
                        rightResourceId.StringId);
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task AddToToManyRelationshipAsync(TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftId,
                rightResourceIds
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Add to to-many relationship");

            AssertHasRelationship(_request.Relationship, relationshipName);

            if (rightResourceIds.Any() && _request.Relationship is HasManyAttribute { IsManyToMany: true } manyToManyRelationship)
            {
                // In the case of a many-to-many relationship, creating a duplicate entry in the join table results in a
                // unique constraint violation. We avoid that by excluding already-existing entries from the set in advance.
                await RemoveExistingIdsFromRelationshipRightSideAsync(manyToManyRelationship, leftId, rightResourceIds, cancellationToken);
            }

            try
            {
                await _repositoryAccessor.AddToToManyRelationshipAsync<TResource, TId>(leftId, rightResourceIds, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                await GetPrimaryResourceByIdAsync(leftId, TopFieldSelection.OnlyIdAttribute, cancellationToken);
                await AssertRightResourcesExistAsync(rightResourceIds, cancellationToken);
                throw;
            }
        }

        private async Task RemoveExistingIdsFromRelationshipRightSideAsync(HasManyAttribute hasManyRelationship, TId leftId,
            ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        {
            TResource leftResource = await GetForHasManyUpdateAsync(hasManyRelationship, leftId, rightResourceIds, cancellationToken);

            object rightValue = _request.Relationship.GetValue(leftResource);
            ICollection<IIdentifiable> existingRightResourceIds = _collectionConverter.ExtractResources(rightValue);

            rightResourceIds.ExceptWith(existingRightResourceIds);
        }

        private async Task<TResource> GetForHasManyUpdateAsync(HasManyAttribute hasManyRelationship, TId leftId, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            QueryLayer queryLayer = _queryLayerComposer.ComposeForHasMany(hasManyRelationship, leftId, rightResourceIds);
            IReadOnlyCollection<TResource> leftResources = await _repositoryAccessor.GetAsync<TResource>(queryLayer, cancellationToken);

            TResource leftResource = leftResources.FirstOrDefault();
            AssertPrimaryResourceExists(leftResource);

            return leftResource;
        }

        protected async Task AssertRightResourcesExistAsync(object rightValue, CancellationToken cancellationToken)
        {
            ICollection<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue);

            if (rightResourceIds.Any())
            {
                QueryLayer queryLayer = _queryLayerComposer.ComposeForGetRelationshipRightIds(_request.Relationship, rightResourceIds);

                List<MissingResourceInRelationship> missingResources =
                    await GetMissingRightResourcesAsync(queryLayer, _request.Relationship, rightResourceIds, cancellationToken).ToListAsync(cancellationToken);

                if (missingResources.Any())
                {
                    throw new ResourcesInRelationshipsNotFoundException(missingResources);
                }
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

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Update resource");

            TResource resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestAttributeValues(resourceFromRequest);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(id, cancellationToken);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceFromDatabase);

            await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.UpdateResource, cancellationToken);

            try
            {
                await _repositoryAccessor.UpdateAsync(resourceFromRequest, resourceFromDatabase, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                await AssertResourcesToAssignInRelationshipsExistAsync(resourceFromRequest, cancellationToken);
                throw;
            }

            TResource afterResourceFromDatabase = await GetPrimaryResourceByIdAsync(id, TopFieldSelection.WithAllAttributes, cancellationToken);

            _resourceChangeTracker.SetFinallyStoredAttributeValues(afterResourceFromDatabase);

            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
            return hasImplicitChanges ? afterResourceFromDatabase : null;
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TId leftId, string relationshipName, object rightValue, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftId,
                relationshipName,
                rightValue
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Service - Set relationship");

            AssertHasRelationship(_request.Relationship, relationshipName);

            TResource resourceFromDatabase = await GetPrimaryResourceForUpdateAsync(leftId, cancellationToken);

            await _resourceDefinitionAccessor.OnPrepareWriteAsync(resourceFromDatabase, WriteOperationKind.SetRelationship, cancellationToken);

            try
            {
                await _repositoryAccessor.SetRelationshipAsync(resourceFromDatabase, rightValue, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                await AssertRightResourcesExistAsync(rightValue, cancellationToken);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                id
            });

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Delete resource");

            try
            {
                await _repositoryAccessor.DeleteAsync<TResource, TId>(id, cancellationToken);
            }
            catch (DataStoreUpdateException)
            {
                await GetPrimaryResourceByIdAsync(id, TopFieldSelection.OnlyIdAttribute, cancellationToken);
                throw;
            }
        }

        /// <inheritdoc />
        public virtual async Task RemoveFromToManyRelationshipAsync(TId leftId, string relationshipName, ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            _traceWriter.LogMethodStart(new
            {
                leftId,
                relationshipName,
                rightResourceIds
            });

            ArgumentGuard.NotNullNorEmpty(relationshipName, nameof(relationshipName));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Repository - Remove from to-many relationship");

            AssertHasRelationship(_request.Relationship, relationshipName);
            var hasManyRelationship = (HasManyAttribute)_request.Relationship;

            TResource resourceFromDatabase = await GetForHasManyUpdateAsync(hasManyRelationship, leftId, rightResourceIds, cancellationToken);

            await AssertRightResourcesExistAsync(rightResourceIds, cancellationToken);

            await _repositoryAccessor.RemoveFromToManyRelationshipAsync(resourceFromDatabase, rightResourceIds, cancellationToken);
        }

        protected async Task<TResource> GetPrimaryResourceByIdAsync(TId id, TopFieldSelection fieldSelection, CancellationToken cancellationToken)
        {
            TResource primaryResource = await TryGetPrimaryResourceByIdAsync(id, fieldSelection, cancellationToken);
            AssertPrimaryResourceExists(primaryResource);

            return primaryResource;
        }

        private async Task<TResource> TryGetPrimaryResourceByIdAsync(TId id, TopFieldSelection fieldSelection, CancellationToken cancellationToken)
        {
            QueryLayer primaryLayer = _queryLayerComposer.ComposeForGetById(id, _request.PrimaryResourceType, fieldSelection);

            IReadOnlyCollection<TResource> primaryResources = await _repositoryAccessor.GetAsync<TResource>(primaryLayer, cancellationToken);
            return primaryResources.SingleOrDefault();
        }

        protected async Task<TResource> GetPrimaryResourceForUpdateAsync(TId id, CancellationToken cancellationToken)
        {
            QueryLayer queryLayer = _queryLayerComposer.ComposeForUpdate(id, _request.PrimaryResourceType);
            var resource = await _repositoryAccessor.GetForUpdateAsync<TResource>(queryLayer, cancellationToken);

            AssertPrimaryResourceExists(resource);
            return resource;
        }

        [AssertionMethod]
        private void AssertPrimaryResourceExists(TResource resource)
        {
            if (resource == null)
            {
                throw new ResourceNotFoundException(_request.PrimaryId, _request.PrimaryResourceType.PublicName);
            }
        }

        [AssertionMethod]
        private void AssertHasRelationship(RelationshipAttribute relationship, string name)
        {
            if (relationship == null)
            {
                throw new RelationshipNotFoundException(name, _request.PrimaryResourceType.PublicName);
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
            IResourceChangeTracker<TResource> resourceChangeTracker, IResourceDefinitionAccessor resourceDefinitionAccessor)
            : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker,
                resourceDefinitionAccessor)
        {
        }
    }
}
