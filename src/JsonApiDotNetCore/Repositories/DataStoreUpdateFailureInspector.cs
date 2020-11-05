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
        Task AssertRightResourcesInRelationshipExistAsync(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds);
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
                ICollection<IIdentifiable> rightResources = TypeHelper.ExtractResources(rightValue);

                var missingResourcesInRelationship = GetMissingRightResourcesAsync(rightResources, relationship);
                await missingResources.AddRangeAsync(missingResourcesInRelationship);
            }

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        public async Task AssertRightResourcesInRelationshipExistAsync(RelationshipAttribute relationship,
            ICollection<IIdentifiable> rightResourceIds)
        {
            var missingResources = await GetMissingRightResourcesAsync(rightResourceIds, relationship).ToListAsync();
            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
            }
        }

        private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingRightResourcesAsync(
            ICollection<IIdentifiable> rightResources, RelationshipAttribute relationship)
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
            var queryLayer = _queryLayerComposer.ComposeForFilterOnResourceIds(typedIds, resourceContext);

            var resources = await _resourceRepositoryAccessor.GetAsync(resourceContext.ResourceType, queryLayer);
            return resources.Select(resource => resource.StringId).ToArray();
        }
    }
}
