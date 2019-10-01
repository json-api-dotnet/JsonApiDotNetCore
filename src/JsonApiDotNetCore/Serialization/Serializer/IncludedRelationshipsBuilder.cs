using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Serializer.Contracts;

namespace JsonApiDotNetCore.Serialization.Serializer
{
    public class IncludedRelationshipsBuilder : ResourceObjectBuilder, IIncludedRelationshipsBuilder
    {
        private readonly HashSet<ResourceObject> _included;
        private readonly ISerializableFields _serializableFields;
        private readonly ILinkBuilder _linkBuilder;

        public IncludedRelationshipsBuilder(ISerializableFields serializableFields,
                                           ILinkBuilder linkBuilder,
                                           IResourceGraph resourceGraph,
                                           IContextEntityProvider provider,
                                           ISerializerBehaviourProvider behaviourProvider) : base(resourceGraph, provider, behaviourProvider)
        {
            _included = new HashSet<ResourceObject>(new ResourceObjectComparer());
            _serializableFields = serializableFields;
            _linkBuilder = linkBuilder;
        }

        /// <inheritdoc/>
        public List<ResourceObject> Build()
        {
            if (_included.Any())
            {
                foreach (var resourceObject in _included)
                {
                    if (resourceObject.Relationships != null)
                    {
                        var pruned = resourceObject.Relationships.Where(p => p.Value.IsPopulated || p.Value.Links != null).ToDictionary(p => p.Key, p => p.Value);
                        if (!pruned.Any())
                            pruned = null;
                        resourceObject.Relationships = pruned;
                    }

                    resourceObject.Links = _linkBuilder.GetResourceLinks(resourceObject.Type, resourceObject.Id);
                }

                return _included.ToList();
            }
            return null;
        }

        /// <inheritdoc/>
        public void IncludeRelationshipChain(List<RelationshipAttribute> inclusionChain, IIdentifiable rootEntity)
        {
            /// We dont have to build a resource object for the root entity because
            /// this one is already encoded in the documents primary data, so we process the chain
            /// starting from the first related entity.
            var relationship = inclusionChain.First();
            var chainRemainder = ShiftChain(inclusionChain);
            var related = _resourceGraph.GetRelationshipValue(rootEntity, relationship);
            ProcessChain(relationship, related, chainRemainder);
        }

        private void ProcessChain(RelationshipAttribute originRelationship, object related, List<RelationshipAttribute> inclusionChain )
        {
            if (related is IEnumerable children)
                foreach (IIdentifiable child in children)
                    ProcessRelationship(originRelationship, child, inclusionChain);
            else
                ProcessRelationship(originRelationship, (IIdentifiable)related, inclusionChain);
        }

        private void ProcessRelationship(RelationshipAttribute originRelationship, IIdentifiable parent, List<RelationshipAttribute> inclusionChain)
        {
            // get the resource object for parent.
            var resourceObject = GetOrBuildResourceObject(parent, originRelationship);
            if (!inclusionChain.Any())
                return;

            var nextRelationship = inclusionChain.First();
            var chainRemainder = inclusionChain.ToList();
            chainRemainder.RemoveAt(0);
            // add the relationship entry in the relationship object.
            var relationshipData = base.GetRelationshipData(nextRelationship, parent);
            resourceObject.Relationships[nextRelationship.PublicRelationshipName] = relationshipData;
            if (relationshipData.HasData)
            {   // if the relationship is populated, continue parsing the chain.
                var related = _resourceGraph.GetRelationshipValue(parent, nextRelationship);
                ProcessChain(nextRelationship, related, chainRemainder);
            }
        }

        private List<RelationshipAttribute> ShiftChain(List<RelationshipAttribute> chain)
        {
            var chainRemainder = chain.ToList();
            chainRemainder.RemoveAt(0);
            return chainRemainder;
        }


        protected override RelationshipData GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            /// We only need a empty relationship object entry here. It will be populated in the
            /// ProcessRelationships method.
            return new RelationshipData { };
        }

        /// <summary>
        /// Gets the resource object for <paramref name="parent"/> by searching the included list.
        /// If it was not already build, it is constructed and added to the included list.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private ResourceObject GetOrBuildResourceObject(IIdentifiable parent, RelationshipAttribute attr)
        {
            /// @TODO: apply sparse field selection using relationship attr.
            var type = parent.GetType();
            var resourceName = _provider.GetContextEntity(type).EntityName;
            var entry = _included.SingleOrDefault(ro => ro.Type == resourceName && ro.Id == parent.StringId);
            if (entry == null)
            {
                entry = BuildResourceObject(parent, _serializableFields.GetAllowedAttributes(type), _serializableFields.GetAllowedRelationships(type));
                _included.Add(entry);
            }
            return entry;
        }
    }
}
