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

        Task AssertRightResourcesInRelationshipExistAsync(RelationshipAttribute relationship,
            object secondaryResourceIds);
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

                var missingResourcesInRelationship =
                    GetMissingResourcesInRelationshipAsync(relationship, rightResources);
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

            var missingResources =
                await GetMissingResourcesInRelationshipAsync(relationship, rightResources).ToListAsync();

            if (missingResources.Any())
            {
                throw new ResourcesInRelationshipsNotFoundException(missingResources);
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

        private async IAsyncEnumerable<MissingResourceInRelationship> GetMissingResourcesInRelationshipAsync(
            RelationshipAttribute relationship, ICollection<IIdentifiable> rightResources)
        {
            if (rightResources.Any())
            {
                var rightIds = rightResources.Select(resource => resource.GetTypedId()).ToHashSet();
                var rightResourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);

                var queryLayer = _queryLayerComposer.ComposeForSecondaryResourceIds(rightIds, rightResourceContext);

                var existingRightResources = await _resourceRepositoryAccessor.GetAsync(relationship.RightType, queryLayer);
                var existingResourceStringIds = existingRightResources.Select(resource => resource.StringId).ToArray();

                foreach (var rightResource in rightResources)
                {
                    if (!existingResourceStringIds.Contains(rightResource.StringId))
                    {
                        var resourceContext = _resourceContextProvider.GetResourceContext(rightResource.GetType());

                        yield return new MissingResourceInRelationship(relationship.PublicName,
                            resourceContext.PublicName, rightResource.StringId);
                    }
                }
            }
        }
    }
}
