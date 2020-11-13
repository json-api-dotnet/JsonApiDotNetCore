using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Repositories
{
    internal sealed class SecondaryResourceResolver : ISecondaryResourceResolver
    {
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IResourceRepositoryAccessor _resourceRepositoryAccessor;

        public SecondaryResourceResolver(IQueryLayerComposer queryLayerComposer, IResourceRepositoryAccessor resourceRepositoryAccessor)
        {
            _queryLayerComposer = queryLayerComposer ?? throw new ArgumentNullException(nameof(queryLayerComposer));
            _resourceRepositoryAccessor = resourceRepositoryAccessor ?? throw new ArgumentNullException(nameof(resourceRepositoryAccessor));
        }

        public async Task<ICollection<MissingResourceInRelationship>> GetMissingResourcesToAssignInRelationships(
            IIdentifiable leftResource)
        {
            var missingResources = new List<MissingResourceInRelationship>();

            foreach (var (queryLayer, relationship) in 
                _queryLayerComposer.ComposeForGetTargetedSecondaryResourceIds(leftResource))
            {
                object rightValue = relationship.GetValue(leftResource);
                ICollection<IIdentifiable> rightResourceIds = TypeHelper.ExtractResources(rightValue);

                var missingResourcesInRelationship = GetMissingRightResourcesAsync(queryLayer, rightResourceIds, relationship);
                await missingResources.AddRangeAsync(missingResourcesInRelationship);
            }

            return missingResources;
        }

        public async Task<ICollection<MissingResourceInRelationship>> GetMissingSecondaryResources(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds)
        {
            var queryLayer = _queryLayerComposer.ComposeForGetRelationshipRightIds(relationship, rightResourceIds);
            var missingResourcesInRelationship = GetMissingRightResourcesAsync(queryLayer, rightResourceIds, relationship).ToListAsync();
            return await missingResourcesInRelationship;
        }

        private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingRightResourcesAsync(
            QueryLayer existingRightResourceIdsQueryLayer, ICollection<IIdentifiable> rightResourceIds, RelationshipAttribute relationship)
        {
            var existingResourceIds = await GetExistingResourceIdsAsync(existingRightResourceIdsQueryLayer);

            foreach (var rightResourceId in rightResourceIds)
            {
                if (!existingResourceIds.Contains(rightResourceId.StringId))
                {
                    yield return new MissingResourceInRelationship(relationship.PublicName,
                        existingRightResourceIdsQueryLayer.ResourceContext.PublicName, rightResourceId.StringId);
                }
            }
        }

        private async Task<ICollection<string>> GetExistingResourceIdsAsync(QueryLayer queryLayer)
        {
            var resources = await _resourceRepositoryAccessor.GetAsync(queryLayer.ResourceContext.ResourceType, queryLayer);
            return resources.Select(resource => resource.StringId).ToArray();
        }
    }
}
