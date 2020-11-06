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
    internal sealed class SecondaryResourceResolver : ISecondaryResourceResolver
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly ITargetedFields _targetedFields;
        private readonly IQueryLayerComposer _queryLayerComposer;
        private readonly IResourceRepositoryAccessor _resourceRepositoryAccessor;

        public SecondaryResourceResolver(IResourceContextProvider resourceContextProvider,
            ITargetedFields targetedFields, IQueryLayerComposer queryLayerComposer,
            IResourceRepositoryAccessor resourceRepositoryAccessor)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _queryLayerComposer = queryLayerComposer ?? throw new ArgumentNullException(nameof(queryLayerComposer));
            _resourceRepositoryAccessor = resourceRepositoryAccessor ?? throw new ArgumentNullException(nameof(resourceRepositoryAccessor));
        }

        public async Task<ICollection<MissingResourceInRelationship>> GetMissingResourcesToAssignInRelationships(IIdentifiable leftResource)
        {
            var missingResources = new List<MissingResourceInRelationship>();

            foreach (var relationship in _targetedFields.Relationships)
            {
                object rightValue = relationship.GetValue(leftResource);
                ICollection<IIdentifiable> rightResources = TypeHelper.ExtractResources(rightValue);

                var missingResourcesInRelationship = GetMissingRightResourcesAsync(rightResources, relationship);
                await missingResources.AddRangeAsync(missingResourcesInRelationship);
            }

            return missingResources;
        }

        public async Task<ICollection<MissingResourceInRelationship>> GetMissingSecondaryResources(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds)
        {
            return await GetMissingRightResourcesAsync(rightResourceIds, relationship).ToListAsync();
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

        public async Task<ICollection<string>> GetExistingResourceIds(ICollection<IIdentifiable> resourceIds, ResourceContext resourceContext)
        {
            if (!resourceIds.Any())
            {
                return Array.Empty<string>();
            }

            var queryLayer = CreateQueryLayerForResourceIds(resourceIds, resourceContext);

            var resources = await _resourceRepositoryAccessor.GetAsync(resourceContext.ResourceType, queryLayer);
            return resources.Select(resource => resource.StringId).ToArray();
        }

        private QueryLayer CreateQueryLayerForResourceIds(IEnumerable<IIdentifiable> resourceIds, ResourceContext resourceContext)
        {
            var idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));

            var typedIds = resourceIds.Select(resource => resource.GetTypedId()).ToArray();
            var filter = _queryLayerComposer.GetFilterOnResourceIds(typedIds, resourceContext);

            return new QueryLayer(resourceContext)
            {
                Filter = filter,
                Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [idAttribute] = null
                }
            };
        }
    }
}
