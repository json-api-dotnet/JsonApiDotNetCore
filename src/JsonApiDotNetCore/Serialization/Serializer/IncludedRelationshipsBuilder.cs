using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{

    public class IncludedRelationshipsBuilder : ResourceObjectBuilder, IIncludedRelationshipsBuilder
    {
        private readonly HashSet<ResourceObject> _included;
        private readonly ISerializableFields _serializableFields;
        private readonly ILinkBuilder _linkBuilder;

        public IncludedRelationshipsBuilder(ISerializableFields serializableFields,
                                           ILinkBuilder linkBuilder,
                                           IResourceGraph resourceGraph,
                                           IContextEntityProvider provider) : base(resourceGraph, provider)
        {
            _included = new HashSet<ResourceObject>(new ResourceObjectComparer());
            _serializableFields = serializableFields;
            _linkBuilder = linkBuilder;
        }

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


        public void IncludeRelationshipChain(List<RelationshipAttribute> inclusionChain, IIdentifiable rootEntity)
        {
            /// we dont have to build a resource object for the root entity because this one is
            /// in the documents primary data. 
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
            var resourceObject = GetOrBuildResourceObject(parent, originRelationship);
            if (!inclusionChain.Any())
                return;

            var nextRelationship = inclusionChain.First();
            var chainRemainder = inclusionChain.ToList();
            chainRemainder.RemoveAt(0);
            var relationshipData = base.GetRelationshipData(nextRelationship, parent);
            resourceObject.Relationships[nextRelationship.PublicRelationshipName] = relationshipData;
            if (relationshipData.HasData)
            {
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
            return new RelationshipData { Links = _linkBuilder.GetRelationshipLinks(relationship, entity) };
        }

        private ResourceObject GetOrBuildResourceObject(IIdentifiable parent, RelationshipAttribute attr)
        {
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
