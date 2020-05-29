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
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.RequestServices.Contracts;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiResourceService<TResource, TId> :
        IResourceService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IResourceRepository<TResource, TId> _repository;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly ICurrentRequest _currentRequest;
        private readonly ILogger _logger;
        private readonly IResourceChangeTracker<TResource> _resourceChangeTracker;
        private readonly IResourceFactory _resourceFactory;
        private readonly IResourceHookExecutor _hookExecutor;

        public JsonApiResourceService(
            IResourceRepository<TResource, TId> repository,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            ICurrentRequest currentRequest,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            IResourceHookExecutor hookExecutor = null)
        {
            _repository = repository;
            _queryLayerComposer = queryLayerComposer;
            _paginationContext = paginationContext;
            _options = options;
            _currentRequest = currentRequest;
            _logger = loggerFactory.CreateLogger<JsonApiResourceService<TResource, TId>>();
            _resourceChangeTracker = resourceChangeTracker;
            _resourceFactory = resourceFactory;
            _hookExecutor = hookExecutor;
        }

        public virtual async Task<TResource> CreateAsync(TResource resource)
        {
            _logger.LogTrace($"Entering {nameof(CreateAsync)}(object).");

            if (_hookExecutor != null)
            {
                resource = _hookExecutor.BeforeCreate(AsList(resource), ResourcePipeline.Post).Single();
            }
            
            await _repository.CreateAsync(resource);

            resource = await GetPrimaryResourceById(resource.Id, true);

            if (_hookExecutor != null)
            {
                _hookExecutor.AfterCreate(AsList(resource), ResourcePipeline.Post);
                resource = _hookExecutor.OnReturn(AsList(resource), ResourcePipeline.Post).Single();
            }

            return resource;
        }

        public virtual async Task DeleteAsync(TId id)
        {
            _logger.LogTrace($"Entering {nameof(DeleteAsync)}('{id}').");

            if (_hookExecutor != null)
            {
                var resource = _resourceFactory.CreateInstance<TResource>();
                resource.Id = id;

                _hookExecutor.BeforeDelete(AsList(resource), ResourcePipeline.Delete);
            }

            var succeeded = await _repository.DeleteAsync(id);

            if (_hookExecutor != null)
            {
                var resource = _resourceFactory.CreateInstance<TResource>();
                resource.Id = id;

                _hookExecutor.AfterDelete(AsList(resource), ResourcePipeline.Delete, succeeded);
            }

            if (!succeeded)
            {
                AssertPrimaryResourceExists(null);
            }
        }

        public virtual async Task<IReadOnlyCollection<TResource>> GetAsync()
        {
            _logger.LogTrace($"Entering {nameof(GetAsync)}().");

            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.Get);

            if (_options.IncludeTotalResourceCount)
            {
                var topFilter = _queryLayerComposer.GetTopFilter();
                _paginationContext.TotalResourceCount = await _repository.CountAsync(topFilter);
            }

            var queryLayer = _queryLayerComposer.Compose(_currentRequest.PrimaryResource);
            var resources = await _repository.GetAsync(queryLayer);

            if (_hookExecutor != null)
            {
                _hookExecutor.AfterRead(resources, ResourcePipeline.Get);
                return _hookExecutor.OnReturn(resources, ResourcePipeline.Get).ToList();
            }

            return resources;
        }

        public virtual async Task<TResource> GetAsync(TId id)
        {
            _logger.LogTrace($"Entering {nameof(GetAsync)}('{id}').");

            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.GetSingle, id.ToString());

            var primaryResource = await GetPrimaryResourceById(id, true);

            if (_hookExecutor != null)
            {
                _hookExecutor.AfterRead(AsList(primaryResource), ResourcePipeline.GetSingle);
                return _hookExecutor.OnReturn(AsList(primaryResource), ResourcePipeline.GetSingle).Single();
            }

            return primaryResource;
        }

        private async Task<TResource> GetPrimaryResourceById(TId id, bool allowTopSparseFieldSet)
        {
            var primaryLayer = _queryLayerComposer.Compose(_currentRequest.PrimaryResource);
            primaryLayer.Sort = null;
            primaryLayer.Pagination = null;
            primaryLayer.Filter = CreateFilterById(id);

            if (!allowTopSparseFieldSet && primaryLayer.Projection != null)
            {
                // Discard any ?fields= or attribute exclusions from ResourceDefinition, because we need the full record.

                while (primaryLayer.Projection.Any(p => p.Key is AttrAttribute))
                {
                    primaryLayer.Projection.Remove(primaryLayer.Projection.First(p => p.Key is AttrAttribute));
                }
            }

            var primaryResources = await _repository.GetAsync(primaryLayer);

            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            return primaryResource;
        }

        private FilterExpression CreateFilterById(TId id)
        {
            var primaryIdAttribute = _currentRequest.PrimaryResource.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));

            return new ComparisonExpression(ComparisonOperator.Equals,
                new ResourceFieldChainExpression(primaryIdAttribute), new LiteralConstantExpression(id.ToString()));
        }

        // triggered by GET /articles/1/relationships/{relationshipName}
        public virtual async Task<TResource> GetRelationshipAsync(TId id, string relationshipName)
        {
            _logger.LogTrace($"Entering {nameof(GetRelationshipAsync)}('{id}', '{relationshipName}').");

            AssertRelationshipExists(relationshipName);

            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.GetRelationship, id.ToString());

            var secondaryLayer = _queryLayerComposer.Compose(_currentRequest.SecondaryResource);

            var secondaryIdAttribute = _currentRequest.SecondaryResource.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));

            secondaryLayer.Include = null;
            secondaryLayer.Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
            {
                [secondaryIdAttribute] = null
            };
            
            var primaryLayer = GetPrimaryLayerForSecondaryEndpoint(secondaryLayer, id);

            var primaryResources = await _repository.GetAsync(primaryLayer);
            
            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            if (_hookExecutor != null)
            {   
                _hookExecutor.AfterRead(AsList(primaryResource), ResourcePipeline.GetRelationship);
                primaryResource = _hookExecutor.OnReturn(AsList(primaryResource), ResourcePipeline.GetRelationship).Single();
            }

            return primaryResource;
        }

        // triggered by GET /articles/1/{relationshipName}
        public virtual async Task<object> GetSecondaryAsync(TId id, string relationshipName)
        {
            _logger.LogTrace($"Entering {nameof(GetSecondaryAsync)}('{id}', '{relationshipName}').");

            AssertRelationshipExists(relationshipName);

            _hookExecutor?.BeforeRead<TResource>(ResourcePipeline.GetRelationship, id.ToString());

            var secondaryLayer = _queryLayerComposer.Compose(_currentRequest.SecondaryResource);
            var primaryLayer = GetPrimaryLayerForSecondaryEndpoint(secondaryLayer, id);

            var primaryResources = await _repository.GetAsync(primaryLayer);
            
            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            if (_hookExecutor != null)
            {   
                _hookExecutor.AfterRead(AsList(primaryResource), ResourcePipeline.GetRelationship);
                primaryResource = _hookExecutor.OnReturn(AsList(primaryResource), ResourcePipeline.GetRelationship).Single();
            }

            return _currentRequest.Relationship.GetValue(primaryResource);
        }

        private QueryLayer GetPrimaryLayerForSecondaryEndpoint(QueryLayer secondaryLayer, TId primaryId)
        {
            var innerInclude = secondaryLayer.Include;
            secondaryLayer.Include = null;

            var primaryIdAttribute =
                _currentRequest.PrimaryResource.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));

            return new QueryLayer(_currentRequest.PrimaryResource)
            {
                Include = RewriteIncludeForSecondaryEndpoint(innerInclude),
                Filter = CreateFilterById(primaryId),
                Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [primaryIdAttribute] = null,
                    [_currentRequest.Relationship] = secondaryLayer
                }
            };
        }

        private IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression relativeInclude)
        {
            if (relativeInclude != null && relativeInclude.Chains.Any())
            {
                var absoluteChains = new List<ResourceFieldChainExpression>();
                foreach (ResourceFieldChainExpression relativeChain in relativeInclude.Chains)
                {
                    var absoluteFieldsInChain = new List<ResourceFieldAttribute>(relativeChain.Fields);
                    absoluteFieldsInChain.Insert(0, _currentRequest.Relationship);

                    var absoluteChain = new ResourceFieldChainExpression(absoluteFieldsInChain);
                    absoluteChains.Add(absoluteChain);
                }

                return new IncludeExpression(absoluteChains);
            }

            return new IncludeExpression(new[] {new ResourceFieldChainExpression(_currentRequest.Relationship)});
        }

        public virtual async Task<TResource> UpdateAsync(TId id, TResource requestResource)
        {
            _logger.LogTrace($"Entering {nameof(UpdateAsync)}('{id}', {(requestResource == null ? "null" : "object")}).");

            TResource databaseResource = await GetPrimaryResourceById(id, false);

            _resourceChangeTracker.SetInitiallyStoredAttributeValues(databaseResource);
            _resourceChangeTracker.SetRequestedAttributeValues(requestResource);

            if (_hookExecutor != null)
            {
                requestResource = _hookExecutor.BeforeUpdate(AsList(requestResource), ResourcePipeline.Patch).Single();
            }

            await _repository.UpdateAsync(requestResource, databaseResource);

            if (_hookExecutor != null)
            {
                _hookExecutor.AfterUpdate(AsList(databaseResource), ResourcePipeline.Patch);
                _hookExecutor.OnReturn(AsList(databaseResource), ResourcePipeline.Patch);
            }

            _repository.FlushFromCache(databaseResource);
            TResource afterResource = await GetPrimaryResourceById(id, false);
            _resourceChangeTracker.SetFinallyStoredAttributeValues(afterResource);

            bool hasImplicitChanges = _resourceChangeTracker.HasImplicitChanges();
            return hasImplicitChanges ? afterResource : null;
        }

        // triggered by PATCH /articles/1/relationships/{relationshipName}
        public virtual async Task UpdateRelationshipAsync(TId id, string relationshipName, object related)
        {
            _logger.LogTrace($"Entering {nameof(UpdateRelationshipAsync)}('{id}', '{relationshipName}', {(related == null ? "null" : "object")}).");

            AssertRelationshipExists(relationshipName);

            var secondaryLayer = _queryLayerComposer.Compose(_currentRequest.SecondaryResource);
            var primaryLayer = GetPrimaryLayerForSecondaryEndpoint(secondaryLayer, id);
            primaryLayer.Projection = null;

            var primaryResources = await _repository.GetAsync(primaryLayer);

            var primaryResource = primaryResources.SingleOrDefault();
            AssertPrimaryResourceExists(primaryResource);

            if (_hookExecutor != null)
            {
                primaryResource = _hookExecutor.BeforeUpdate(AsList(primaryResource), ResourcePipeline.PatchRelationship).Single();
            }

            string[] relationshipIds = null;
            if (related != null)
            {
                relationshipIds = _currentRequest.Relationship is HasOneAttribute
                    ? new[] {((IIdentifiable) related).StringId}
                    : ((IEnumerable<IIdentifiable>) related).Select(e => e.StringId).ToArray();
            }

            await _repository.UpdateRelationshipsAsync(primaryResource, _currentRequest.Relationship, relationshipIds ?? Array.Empty<string>());

            if (_hookExecutor != null && primaryResource != null)
            {
                _hookExecutor.AfterUpdate(AsList(primaryResource), ResourcePipeline.PatchRelationship);
            }
        }

        private void AssertPrimaryResourceExists(TResource resource)
        {
            if (resource == null)
            {
                throw new ResourceNotFoundException(_currentRequest.PrimaryId, _currentRequest.PrimaryResource.ResourceName);
            }
        }

        private void AssertRelationshipExists(string relationshipName)
        {
            var relationship = _currentRequest.Relationship;
            if (relationship == null)
            {
                throw new RelationshipNotFoundException(relationshipName, _currentRequest.PrimaryResource.ResourceName);
            }
        }

        private static List<TResource> AsList(TResource resource)
        {
            return new List<TResource> { resource };
        }
    }

    /// <summary>
    /// No mapping with integer as default
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class JsonApiResourceService<TResource> : JsonApiResourceService<TResource, int>,
        IResourceService<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public JsonApiResourceService(
            IResourceRepository<TResource, int> repository,
            IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext,
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            ICurrentRequest currentRequest,
            IResourceChangeTracker<TResource> resourceChangeTracker,
            IResourceFactory resourceFactory,
            IResourceHookExecutor hookExecutor = null)
            : base(repository, queryLayerComposer, paginationContext, options, loggerFactory, currentRequest,
                resourceChangeTracker, resourceFactory, hookExecutor)
        { }
    }
}
