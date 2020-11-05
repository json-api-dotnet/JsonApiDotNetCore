using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Repositories
{
    public interface IDataStoreUpdateFailureInspector
    {
        Task AssertRightResourcesInRelationshipsExistAsync(IIdentifiable leftResource);
        Task AssertRightResourcesInRelationshipExistAsync(RelationshipAttribute relationship, object secondaryResourceIds);
        Task AssertResourcesExist(Type resourceType, ISet<IIdentifiable> resourceIds);
    }

    internal sealed class DataStoreUpdateFailureInspector : IDataStoreUpdateFailureInspector
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly ITargetedFields _targetedFields;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IResourceRepositoryAccessor _resourceRepositoryAccessor;

        public DataStoreUpdateFailureInspector(IResourceContextProvider resourceContextProvider,
            ITargetedFields targetedFields, IQueryLayerComposer queryLayerComposer,
            IResourceRepositoryAccessor resourceRepositoryAccessor)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _queryLayerComposer = queryLayerComposer ?? throw new ArgumentNullException(nameof(queryLayerComposer));
            _resourceRepositoryAccessor = resourceRepositoryAccessor ?? throw new ArgumentNullException(nameof(resourceRepositoryAccessor));
        }

        public async Task AssertRightResourcesInRelationshipsExistAsync(IIdentifiable leftResource)
        {
            var missingResources = new List<MissingResourceInRelationship>();

            foreach (var relationship in _targetedFields.Relationships)
            {
                object rightValue = relationship.GetValue(leftResource);
                ICollection<IIdentifiable> rightResources = ExtractResources(rightValue);

                var missingResourcesInRelationship = GetMissingResourcesAsync(relationship, rightResources);
                await missingResources.AddRangeAsync(missingResourcesInRelationship);
            }

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        public async Task AssertRightResourcesInRelationshipExistAsync(RelationshipAttribute relationship,
            object secondaryResourceIds)
        {
            ICollection<IIdentifiable> rightResources = ExtractResources(secondaryResourceIds);

            var missingResources = await GetMissingResourcesAsync(relationship, rightResources).ToListAsync();
            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        public async Task AssertResourcesExist(Type resourceType, ISet<IIdentifiable> resourceIds)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            var existingResourceIds = await GetExistingResourceIds(resourceIds, resourceContext);

            if (existingResourceIds.Count < resourceIds.Count)
            {
                throw new DataStoreUpdateException($"One or more related resources of type '{resourceType}' do not exist.");
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

        private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingResourcesAsync(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResources)
        {
            var rightResourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);
            var existingResourceIds = await GetExistingResourceIds(rightResources, rightResourceContext);

            foreach (var rightResource in rightResources)
            {
                if (!existingResourceIds.Contains(rightResource.StringId))
                {
                    var resourceContext = _resourceContextProvider.GetResourceContext(rightResource.GetType());

                    yield return new MissingResourceInRelationship(relationship.PublicName,
                        resourceContext.PublicName, rightResource.StringId);
                }
            }
        }

        private async Task<ICollection<string>> GetExistingResourceIds(ICollection<IIdentifiable> resourceIds, ResourceContext resourceContext)
        {
            if (!resourceIds.Any())
            {
                return Array.Empty<string>();
            }

            var typedIds = resourceIds.Select(resource => resource.GetTypedId()).ToHashSet();
            var queryLayer = _queryLayerComposer.ComposeForSecondaryResourceIds(typedIds, resourceContext);

            var resources = await _resourceRepositoryAccessor.GetAsync(resourceContext.ResourceType, queryLayer);
            return resources.Select(resource => resource.StringId).ToArray();
        }
    }
}
