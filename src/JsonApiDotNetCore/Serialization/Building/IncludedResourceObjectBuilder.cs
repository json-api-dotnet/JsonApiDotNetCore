using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    [PublicAPI]
    public class IncludedResourceObjectBuilder : ResourceObjectBuilder, IIncludedResourceObjectBuilder
    {
        private readonly HashSet<ResourceObject> _included;
        private readonly IFieldsToSerialize _fieldsToSerialize;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly SparseFieldSetCache _sparseFieldSetCache;

        public IncludedResourceObjectBuilder(IFieldsToSerialize fieldsToSerialize, ILinkBuilder linkBuilder, IResourceContextProvider resourceContextProvider,
            IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
            IResourceObjectBuilderSettingsProvider settingsProvider)
            : base(resourceContextProvider, settingsProvider.Get())
        {
            ArgumentGuard.NotNull(fieldsToSerialize, nameof(fieldsToSerialize));
            ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

            _included = new HashSet<ResourceObject>(ResourceIdentifierObjectComparer.Instance);
            _fieldsToSerialize = fieldsToSerialize;
            _linkBuilder = linkBuilder;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _sparseFieldSetCache = new SparseFieldSetCache(constraintProviders, resourceDefinitionAccessor);
        }

        /// <inheritdoc />
        public IList<ResourceObject> Build()
        {
            if (_included.Any())
            {
                // cleans relationship dictionaries and adds links of resources.
                foreach (ResourceObject resourceObject in _included)
                {
                    if (resourceObject.Relationships != null)
                    {
                        UpdateRelationships(resourceObject);
                    }

                    resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                }

                return _included.ToArray();
            }

            return null;
        }

        private void UpdateRelationships(ResourceObject resourceObject)
        {
            foreach (string relationshipName in resourceObject.Relationships.Keys.ToArray())
            {
                ResourceContext resourceContext = ResourceContextProvider.GetResourceContext(resourceObject.Type);
                RelationshipAttribute relationship = resourceContext.Relationships.Single(rel => rel.PublicName == relationshipName);

                if (!IsRelationshipInSparseFieldSet(relationship))
                {
                    resourceObject.Relationships.Remove(relationshipName);
                }
            }

            resourceObject.Relationships = PruneRelationshipEntries(resourceObject);
        }

        private static IDictionary<string, RelationshipEntry> PruneRelationshipEntries(ResourceObject resourceObject)
        {
            Dictionary<string, RelationshipEntry> pruned = resourceObject.Relationships.Where(pair => pair.Value.IsPopulated || pair.Value.Links != null)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return !pruned.Any() ? null : pruned;
        }

        private bool IsRelationshipInSparseFieldSet(RelationshipAttribute relationship)
        {
            ResourceContext resourceContext = ResourceContextProvider.GetResourceContext(relationship.LeftType);

            IReadOnlyCollection<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceContext);
            return fieldSet.Contains(relationship);
        }

        /// <inheritdoc />
        public override ResourceObject Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes = null,
            IReadOnlyCollection<RelationshipAttribute> relationships = null)
        {
            ResourceObject resourceObject = base.Build(resource, attributes, relationships);

            resourceObject.Meta = _resourceDefinitionAccessor.GetMeta(resource.GetType(), resource);

            return resourceObject;
        }

        /// <inheritdoc />
        public void IncludeRelationshipChain(IReadOnlyCollection<RelationshipAttribute> inclusionChain, IIdentifiable rootResource)
        {
            ArgumentGuard.NotNull(inclusionChain, nameof(inclusionChain));
            ArgumentGuard.NotNull(rootResource, nameof(rootResource));

            // We don't have to build a resource object for the root resource because
            // this one is already encoded in the documents primary data, so we process the chain
            // starting from the first related resource.
            RelationshipAttribute relationship = inclusionChain.First();
            IList<RelationshipAttribute> chainRemainder = ShiftChain(inclusionChain);
            object related = relationship.GetValue(rootResource);
            ProcessChain(related, chainRemainder);
        }

        private void ProcessChain(object related, IList<RelationshipAttribute> inclusionChain)
        {
            if (related is IEnumerable children)
            {
                foreach (IIdentifiable child in children)
                {
                    ProcessRelationship(child, inclusionChain);
                }
            }
            else
            {
                ProcessRelationship((IIdentifiable)related, inclusionChain);
            }
        }

        private void ProcessRelationship(IIdentifiable parent, IList<RelationshipAttribute> inclusionChain)
        {
            // get the resource object for parent.
            ResourceObject resourceObject = GetOrBuildResourceObject(parent);

            if (!inclusionChain.Any())
            {
                return;
            }

            RelationshipAttribute nextRelationship = inclusionChain.First();
            List<RelationshipAttribute> chainRemainder = inclusionChain.ToList();
            chainRemainder.RemoveAt(0);

            string nextRelationshipName = nextRelationship.PublicName;
            IDictionary<string, RelationshipEntry> relationshipsObject = resourceObject.Relationships;

            // add the relationship entry in the relationship object.
            if (!relationshipsObject.TryGetValue(nextRelationshipName, out RelationshipEntry relationshipEntry))
            {
                relationshipEntry = GetRelationshipData(nextRelationship, parent);
                relationshipsObject[nextRelationshipName] = relationshipEntry;
            }

            relationshipEntry.Data = GetRelatedResourceLinkage(nextRelationship, parent);

            if (relationshipEntry.HasResource)
            {
                // if the relationship is set, continue parsing the chain.
                object related = nextRelationship.GetValue(parent);
                ProcessChain(related, chainRemainder);
            }
        }

        private IList<RelationshipAttribute> ShiftChain(IReadOnlyCollection<RelationshipAttribute> chain)
        {
            List<RelationshipAttribute> chainRemainder = chain.ToList();
            chainRemainder.RemoveAt(0);
            return chainRemainder;
        }

        /// <summary>
        /// We only need an empty relationship object entry here. It will be populated in the ProcessRelationships method.
        /// </summary>
        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable resource)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(resource, nameof(resource));

            return new RelationshipEntry
            {
                Links = _linkBuilder.GetRelationshipLinks(relationship, resource)
            };
        }

        /// <summary>
        /// Gets the resource object for <paramref name="parent" /> by searching the included list. If it was not already built, it is constructed and added to
        /// the inclusion list.
        /// </summary>
        private ResourceObject GetOrBuildResourceObject(IIdentifiable parent)
        {
            Type type = parent.GetType();
            string resourceName = ResourceContextProvider.GetResourceContext(type).PublicName;
            ResourceObject entry = _included.SingleOrDefault(ro => ro.Type == resourceName && ro.Id == parent.StringId);

            if (entry == null)
            {
                entry = Build(parent, _fieldsToSerialize.GetAttributes(type), _fieldsToSerialize.GetRelationships(type));
                _included.Add(entry);
            }

            return entry;
        }
    }
}
