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
using JsonApiDotNetCore.Queries.Expressions;
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
        private readonly IResourceRepositoryAccessor _repositoryAccessor;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly TraceLogWriter<JsonApiResourceService<TResource, TId>> _traceWriter;
        private readonly IJsonApiRequest _request;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceFactory _resourceFactory;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceHookExecutorFacade _hookExecutor;

        public JsonApiResourceService(
            IResourceRepository<TResource, TId> repository,
            IResourceRepositoryAccessor repositoryAccessor,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceHookExecutorFacade hookExecutor)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _repositoryAccessor = repositoryAccessor ?? throw new ArgumentNullException(nameof(repositoryAccessor));
            _queryLayerComposer = queryLayerComposer ?? throw new ArgumentNullException(nameof(queryLayerComposer));
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _traceWriter = new TraceLogWriter<JsonApiResourceService<TResource, TId>>(loggerFactory);
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _resourceChangeTracker = resourceChangeTracker ?? throw new ArgumentNullException(nameof(resourceChangeTracker));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _hookExecutor = hookExecutor ?? throw new ArgumentNullException(nameof(hookExecutor));
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync()
        {
            _traceWriter.LogMethodStart();

            _hookExecutor.BeforeReadMany<TResource>();

            if (_options.IncludeTotalResourceCount)
            {
                var topFilter = _queryLayerComposer.GetTopFilter();
                _paginationContext.TotalResourceCount = await _repository.CountAsync(topFilter);

                if (_paginationContext.TotalResourceCount == 0)
                {
                    return Array.Empty<TResource>();
                }
            }

            var queryLayer = _queryLayerComposer.Compose(_request.PrimaryResource);
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

            var primaryResource = await GetPrimaryResourceById(id, TopFieldSelection.PreserveExisting);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetSingle);
            _hookExecutor.OnReturnSingle(primaryResource, ResourcePipeline.GetSingle);

            return primaryResource;
        }

        /// <inheritdoc />
        public virtual async Task<object> GetSecondaryAsync(TId id, string relationshipName)
        {
            _traceWriter.LogMethodStart(new {id, relationshipName});
            AssertRelationshipExists(relationshipName);

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetRelationship);

            var secondaryLayer = _queryLayerComposer.Compose(_request.SecondaryResource);
            var primaryLayer = _queryLayerComposer.WrapLayerForSecondaryEndpoint(secondaryLayer, _request.PrimaryResource, id, _request.Relationship);

            if (_request.IsCollection && _options.IncludeTotalResourceCount)
            {
                // TODO: Consider support for pagination links on secondary resource collection. This requires to call Count() on the inverse relationship (which may not exist).
                // For /blogs/{id}/articles we need to execute Count(Articles.Where(article => article.Blog.Id == 1 && article.Blog.existingFilter))) to determine TotalResourceCount.
                // This also means we need to invoke ResourceRepository<Article>.CountAsync() from ResourceService<Blog>.
                // And we should call BlogResourceDefinition.OnApplyFilter to filter out soft-deleted blogs and translate from equals('IsDeleted','false') to equals('Blog.IsDeleted','false')
            }

            var primaryResources = await _repository.GetAsync(primaryLayer);
            
            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            _hookExecutor.AfterReadSingle(primaryResource, ResourcePipeline.GetRelationship);

            var secondaryResourceOrResources = _request.Relationship.GetValue(primaryResource);

            if (secondaryResourceOrResources is ICollection secondaryResources &&
                secondaryLayer.Pagination?.PageSize != null &&
                secondaryLayer.Pagination.PageSize.Value == secondaryResources.Count)
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

            AssertRelationshipExists(relationshipName);

            _hookExecutor.BeforeReadSingle<TResource, TId>(id, ResourcePipeline.GetRelationship);

            var secondaryLayer = _queryLayerComposer.Compose(_request.SecondaryResource);
            secondaryLayer.Projection = _queryLayerComposer.GetSecondaryProjectionForRelationshipEndpoint(_request.SecondaryResource);
            secondaryLayer.Include = null;

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

            var resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestedAttributeValues(resourceFromRequest);

            var defaultResource = _resourceFactory.CreateInstance<TResource>();
            defaultResource.Id = resource.Id;

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(defaultResource);

            _hookExecutor.BeforeCreate(resourceFromRequest);

            try
            {
                await _repository.CreateAsync(resourceFromRequest);
            }
            catch (DataStoreUpdateException)
            {
                var existingResource = await TryGetPrimaryResourceById(resource.Id, TopFieldSelection.OnlyIdAttribute);
                if (existingResource != null)
                {
                    throw new ResourceAlreadyExistsException(resource.StringId, _request.PrimaryResource.PublicName);
                }

                await AssertRightResourcesInRelationshipsExistAsync(_targetedFields.Relationships, resourceFromRequest);
                throw;
            }

            var resourceFromDatabase = await GetPrimaryResourceById(resourceFromRequest.Id, TopFieldSelection.WithAllAttributes);

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

        /// <inheritdoc />
        public async Task AddToToManyRelationshipAsync(TId id, string relationshipName, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new { id, secondaryResourceIds });
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            AssertRelationshipExists(relationshipName);
            AssertRelationshipIsToMany();
    
            if (secondaryResourceIds.Any())
            {
                try
                {
                    await _repository.AddToToManyRelationshipAsync(id, secondaryResourceIds);
                }
                catch (DataStoreUpdateException)
                {
                    await GetPrimaryResourceById(id, TopFieldSelection.OnlyIdAttribute);
                    await AssertRightResourcesInRelationshipExistAsync(_request.Relationship, secondaryResourceIds);

                    throw;
                }
            }
        }

        /// <inheritdoc />
        public virtual async Task<TResource> UpdateAsync(TId id, TResource resource)
        {
            _traceWriter.LogMethodStart(new {id, resource});
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            AssertResourceIdIsNotTargeted();

            var resourceFromRequest = resource;
            _resourceChangeTracker.SetRequestedAttributeValues(resourceFromRequest);

            _hookExecutor.BeforeUpdateResource(resourceFromRequest);

            TResource resourceFromDatabase = await GetPrimaryResourceById(id, TopFieldSelection.WithAllAttributes);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(resourceFromDatabase);

            try
            {
                await _repository.UpdateAsync(resourceFromRequest, resourceFromDatabase);
            }
            catch (DataStoreUpdateException)
            {
                await AssertRightResourcesInRelationshipsExistAsync(_targetedFields.Relationships, resourceFromRequest);
                throw;
            }

            TResource afterResourceFromDatabase = await GetPrimaryResourceById(id, TopFieldSelection.WithAllAttributes);

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

        private void AssertResourceIdIsNotTargeted()
        {
            if (_targetedFields.Attributes.Any(attribute => attribute.Property.Name == nameof(Identifiable.Id)))
            {
                throw new ResourceIdIsReadOnlyException();
            }
        }

        /// <inheritdoc />
        public virtual async Task SetRelationshipAsync(TId id, string relationshipName, object secondaryResourceIds)
        {
             _traceWriter.LogMethodStart(new {id, relationshipName, secondaryResourceIds});
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));

            AssertRelationshipExists(relationshipName);

            await _hookExecutor.BeforeUpdateRelationshipAsync(id,
                async () => await GetPrimaryResourceById(id, TopFieldSelection.WithAllAttributes));

            try
            {
                await _repository.SetRelationshipAsync(id, secondaryResourceIds);
            }
            catch (DataStoreUpdateException)
            {
                await GetPrimaryResourceById(id, TopFieldSelection.OnlyIdAttribute);
                await AssertRightResourcesInRelationshipExistAsync(_request.Relationship, secondaryResourceIds);

                throw;
            }

            await _hookExecutor.AfterUpdateRelationshipAsync(id,
                async () => await GetPrimaryResourceById(id, TopFieldSelection.WithAllAttributes));
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id)
        {
            _traceWriter.LogMethodStart(new {id});

            await _hookExecutor.BeforeDeleteAsync(id,
                async () => await GetPrimaryResourceById(id, TopFieldSelection.WithAllAttributes));

            try
            {
                await _repository.DeleteAsync(id);
            }
            catch (DataStoreUpdateException)
            {
                await GetPrimaryResourceById(id, TopFieldSelection.OnlyIdAttribute);
                throw;
            }

            await _hookExecutor.AfterDeleteAsync(id,
                async () => await GetPrimaryResourceById(id, TopFieldSelection.WithAllAttributes));
        }

        /// <inheritdoc />
        public async Task RemoveFromToManyRelationshipAsync(TId id, string relationshipName, ISet<IIdentifiable> secondaryResourceIds)
        {
            _traceWriter.LogMethodStart(new {id, relationshipName, secondaryResourceIds});
            if (relationshipName == null) throw new ArgumentNullException(nameof(relationshipName));
            if (secondaryResourceIds == null) throw new ArgumentNullException(nameof(secondaryResourceIds));

            AssertRelationshipExists(relationshipName);
            AssertRelationshipIsToMany();

            try
            {
                await _repository.RemoveFromToManyRelationshipAsync(id, secondaryResourceIds);
            }
            catch (DataStoreUpdateException)
            {
                await GetPrimaryResourceById(id, TopFieldSelection.OnlyIdAttribute);
                throw;
            }
        }

        private async Task<TResource> GetPrimaryResourceById(TId id, TopFieldSelection fieldSelection)
        {
            var primaryResource = await TryGetPrimaryResourceById(id, fieldSelection);

            AssertPrimaryResourceExists(primaryResource);

            return primaryResource;
        }

        private async Task<TResource> TryGetPrimaryResourceById(TId id, TopFieldSelection fieldSelection)
        {
            var primaryLayer = _queryLayerComposer.Compose(_request.PrimaryResource);
            primaryLayer.Sort = null;
            primaryLayer.Pagination = null;
            primaryLayer.Filter = IncludeFilterById(id, primaryLayer.Filter);

            if (fieldSelection == TopFieldSelection.OnlyIdAttribute)
            {
                var idAttribute = _request.PrimaryResource.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));
                primaryLayer.Projection = new Dictionary<ResourceFieldAttribute, QueryLayer> {{idAttribute, null}};
            }
            else if (fieldSelection == TopFieldSelection.WithAllAttributes && primaryLayer.Projection != null)
            {
                // Discard any top-level ?fields= or attribute exclusions from resource definition, because we need the full database row.
                while (primaryLayer.Projection.Any(p => p.Key is AttrAttribute))
                {
                    primaryLayer.Projection.Remove(primaryLayer.Projection.First(p => p.Key is AttrAttribute));
                }
            }

            var primaryResources = await _repository.GetAsync(primaryLayer);
            return primaryResources.SingleOrDefault();
        }

        private FilterExpression IncludeFilterById(TId id, FilterExpression existingFilter)
        {
            var primaryIdAttribute = _request.PrimaryResource.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));

            FilterExpression filterById = new ComparisonExpression(ComparisonOperator.Equals,
                new ResourceFieldChainExpression(primaryIdAttribute), new LiteralConstantExpression(id.ToString()));

            return existingFilter == null
                ? filterById
                : new LogicalExpression(LogicalOperator.And, new[] { filterById, existingFilter });
        }

        private async Task AssertRightResourcesInRelationshipsExistAsync(IEnumerable<RelationshipAttribute> relationships, TResource leftResource)
        {
            var missingResources = new List<MissingResourceInRelationship>();

            foreach (var relationship in relationships)
            {
                object rightValue = relationship.GetValue(leftResource);
                ICollection<IIdentifiable> rightResources = ExtractResources(rightValue);

                var missingResourcesInRelationship = GetMissingResourcesInRelationshipAsync(relationship, rightResources);
                await missingResources.AddRangeAsync(missingResourcesInRelationship);
            }

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipAssignmentsNotFoundException(missingResources);
            }
        }

        private async Task AssertRightResourcesInRelationshipExistAsync(RelationshipAttribute relationship, object secondaryResourceIds)
        {
            ICollection<IIdentifiable> rightResources = ExtractResources(secondaryResourceIds);

            var missingResources = await GetMissingResourcesInRelationshipAsync(relationship, rightResources).ToListAsync();
            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipAssignmentsNotFoundException(missingResources);
            }
        }

        private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingResourcesInRelationshipAsync(
            RelationshipAttribute relationship, ICollection<IIdentifiable> rightResources)
        {
            if (rightResources.Any())
            {
                var rightIds = rightResources.Select(resource => resource.GetTypedId());
                var existingResourceStringIds = await GetSecondaryResourceStringIdsAsync(relationship.RightType, rightIds);

                foreach (var rightResource in rightResources)
                {
                    if (existingResourceStringIds.Contains(rightResource.StringId))
                    {
                        continue;
                    }

                    var resourceContext = _resourceContextProvider.GetResourceContext(rightResource.GetType());

                    yield return new MissingResourceInRelationship(relationship.PublicName,
                        resourceContext.PublicName, rightResource.StringId);
                }
            }
        }

        private static ICollection<IIdentifiable> ExtractResources(object value)
        {
            if (value is IEnumerable<IIdentifiable> resources)
            {
                return resources.ToList();
            }

            if (value is IIdentifiable resource)
            {
                return new[] {resource};
            }

            return Array.Empty<IIdentifiable>();
        }

        private async Task<ICollection<string>> GetSecondaryResourceStringIdsAsync(Type resourceType, IEnumerable<object> typedIds)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            var idValues = typedIds.Select(id => id.ToString()).ToArray();
            var idsFilter = CreateFilterByIds(idValues, resourceContext);

            var queryLayer = new QueryLayer(resourceContext)
            {
                Filter = idsFilter
            };

            var resources = await _repositoryAccessor.GetAsync(resourceType, queryLayer);
            return resources.Select(resource => resource.StringId).ToArray();
        }

        private FilterExpression CreateFilterByIds(ICollection<string> ids, ResourceContext resourceContext)
        {
            var idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            var idChain = new ResourceFieldChainExpression(idAttribute);

            if (ids.Count == 1)
            {
                var constant = new LiteralConstantExpression(ids.Single());
                return new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
            }

            var constants = ids.Select(id => new LiteralConstantExpression(id)).ToList();
            return new EqualsAnyOfExpression(idChain, constants);
        }

        private void AssertPrimaryResourceExists(TResource resource)
        {
            if (resource == null)
            {
                throw new ResourceNotFoundException(_request.PrimaryId, _request.PrimaryResource.PublicName);
            }
        }

        private void AssertRelationshipExists(string relationshipName)
        {
            if (_request.Relationship == null)
            {
                throw new RelationshipNotFoundException(relationshipName, _request.PrimaryResource.PublicName);
            }
        }

        private void AssertRelationshipIsToMany()
        {
            var relationship = _request.Relationship;
            if (!(relationship is HasManyAttribute))
            {
                throw new ToManyRelationshipRequiredException(relationship.PublicName);
            }
        }

        private enum TopFieldSelection
        {
            /// <summary>
            /// Preserves included relationships, but selects all resource attributes.
            /// </summary>
            WithAllAttributes,
            /// <summary>
            /// Discards any included relationships and selects only resource ID.
            /// </summary>
            OnlyIdAttribute,
            /// <summary>
            /// Preserves the existing selection of attributes and/or relationships.
            /// </summary>
            PreserveExisting
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
            IResourceRepositoryAccessor repositoryAccessor,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IJsonApiRequest request,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider,
            IResourceHookExecutorFacade hookExecutor)
            : base(repository, repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory,
                request, resourceChangeTracker, resourceFactory, targetedFields, resourceContextProvider, hookExecutor)
        { }
    }
}
