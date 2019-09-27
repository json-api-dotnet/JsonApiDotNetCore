using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Deserializer.Contracts;

namespace JsonApiDotNetCore.Serialization.Deserializer
{
    public class ClientDeserializer : DocumentParser, IClientDeserializer
    {
        public ClientDeserializer(IContextEntityProvider provider) : base(provider) { }

        public DeserializedSingleResponse<TResource> DeserializeSingle<TResource>(string body) where TResource : class, IIdentifiable
        {
            var entity = base.Deserialize(body);
            return new DeserializedSingleResponse<TResource>()
            {
                Links = _document.Links,
                Meta = _document.Meta,
                Data = entity == null ? null : (TResource)entity,
                JsonApi = null,
                Errors = null
            };
        }

        public DeserializedListResponse<TResource> DeserializeList<TResource>(string body) where TResource : class, IIdentifiable
        {
            var entities = base.Deserialize(body);
            return new DeserializedListResponse<TResource>()
            {
                Links = _document.Links,
                Meta = _document.Meta,
                Data = entities == null ? null : ((List<IIdentifiable>)entities).Cast<TResource>().ToList(),
                JsonApi = null,
                Errors = null
            };
        }

        protected override void AfterProcessField(IIdentifiable entity, IResourceField field, RelationshipData data = null)
        {
            if (field is AttrAttribute)
                return;

            // can't provide any more data other than the rios since it is not contained in the included section
            if (_document.Included == null || _document.Included.Count == 0)
                return;

            if (field is HasOneAttribute hasOneAttr)
            {
                var rio = data.SingleData;
                if (rio == null)
                    hasOneAttr.SetValue(entity, null);
                else
                    hasOneAttr.SetValue(entity, GetIncludedRelationship(hasOneAttr, rio));
            }
            else if (field is HasManyAttribute hasManyAttr)
            {
                var values = TypeHelper.CreateListFor(hasManyAttr.DependentType);
                foreach (var rio in data.ManyData)
                    values.Add(GetIncludedRelationship(hasManyAttr, rio));

                hasManyAttr.SetValue(entity, values);
            }
        }

        private IIdentifiable GetIncludedRelationship(RelationshipAttribute relationshipAttr, ResourceIdentifierObject relatedResourceIdentifier)
        {
            var relatedInstance = relationshipAttr.DependentType.New<IIdentifiable>();
            relatedInstance.StringId = relatedResourceIdentifier.Id;

            var includedResource = GetLinkedResource(relatedResourceIdentifier);
            if (includedResource == null)
                return relatedInstance;

            var contextEntity = _provider.GetContextEntity(relatedResourceIdentifier.Type);
            if (contextEntity == null)
                throw new InvalidOperationException($"Included type '{relationshipAttr.DependentType}' is not a registered json:api resource.");

            SetAttributes(relatedInstance, includedResource.Attributes, contextEntity.Attributes);
            SetRelationships(relatedInstance, includedResource.Relationships, contextEntity.Relationships);
            return relatedInstance;
        }


        private ResourceObject GetLinkedResource(ResourceIdentifierObject relatedResourceIdentifier)
        {
            try
            {
                return _document.Included.SingleOrDefault(r => r.Type == relatedResourceIdentifier.Type && r.Id == relatedResourceIdentifier.Id);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"A compound document MUST NOT include more than one resource object for each type and id pair."
                        + $"The duplicate pair was '{relatedResourceIdentifier.Type}, {relatedResourceIdentifier.Id}'", e);
            }
        }
    }
}
