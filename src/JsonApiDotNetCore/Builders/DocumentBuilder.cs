using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Builders
{
    /// <inheritdoc />
    public class DocumentBuilder : IDocumentBuilder
    {
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IResourceGraph _resourceGraph;
        private readonly IRequestMeta _requestMeta;
        private readonly DocumentBuilderOptions _documentBuilderOptions;
        private readonly IScopedServiceProvider _scopedServiceProvider;

        public DocumentBuilder(
            IJsonApiContext jsonApiContext,
            IRequestMeta requestMeta = null,
            IDocumentBuilderOptionsProvider documentBuilderOptionsProvider = null,
            IScopedServiceProvider scopedServiceProvider = null)
        {
            _jsonApiContext = jsonApiContext;
            _resourceGraph = jsonApiContext.ResourceGraph;
            _requestMeta = requestMeta;
            _documentBuilderOptions = documentBuilderOptionsProvider?.GetDocumentBuilderOptions() ?? new DocumentBuilderOptions();
            _scopedServiceProvider = scopedServiceProvider;
        }

        /// <inheritdoc />
        public Document Build(IIdentifiable entity)
        {
            var contextEntity = _resourceGraph.GetContextEntity(entity.GetType());

            var resourceDefinition = _scopedServiceProvider?.GetService(contextEntity.ResourceType) as IResourceDefinition;
            var document = new Document
            {
                Data = GetData(contextEntity, entity, resourceDefinition),
                Meta = GetMeta(entity)
            };

            if (ShouldIncludePageLinks(contextEntity))
                document.Links = _jsonApiContext.PageManager.GetPageLinks(new LinkBuilder(_jsonApiContext));

            document.Included = AppendIncludedObject(document.Included, contextEntity, entity);

            return document;
        }

        /// <inheritdoc />
        public Documents Build(IEnumerable<IIdentifiable> entities)
        {
            var entityType = entities.GetElementType();
            var contextEntity = _resourceGraph.GetContextEntity(entityType);
            var resourceDefinition = _scopedServiceProvider?.GetService(contextEntity.ResourceType) as IResourceDefinition;

            var enumeratedEntities = entities as IList<IIdentifiable> ?? entities.ToList();
            var documents = new Documents
            {
                Data = new List<ResourceObject>(),
                Meta = GetMeta(enumeratedEntities.FirstOrDefault())
            };

            if (ShouldIncludePageLinks(contextEntity))
                documents.Links = _jsonApiContext.PageManager.GetPageLinks(new LinkBuilder(_jsonApiContext));

            foreach (var entity in enumeratedEntities)
            {
                documents.Data.Add(GetData(contextEntity, entity, resourceDefinition));
                documents.Included = AppendIncludedObject(documents.Included, contextEntity, entity);
            }

            return documents;
        }

        private Dictionary<string, object> GetMeta(IIdentifiable entity)
        {
            var builder = _jsonApiContext.MetaBuilder;
            if (_jsonApiContext.Options.IncludeTotalRecordCount && _jsonApiContext.PageManager.TotalRecords != null)
                builder.Add("total-records", _jsonApiContext.PageManager.TotalRecords);

            if (_requestMeta != null)
                builder.Add(_requestMeta.GetMeta());

            if (entity != null && entity is IHasMeta metaEntity)
                builder.Add(metaEntity.GetMeta(_jsonApiContext));

            var meta = builder.Build();
            if (meta.Count > 0)
                return meta;

            return null;
        }

        private bool ShouldIncludePageLinks(ContextEntity entity) => entity.Links.HasFlag(Link.Paging);

        private List<ResourceObject> AppendIncludedObject(List<ResourceObject> includedObject, ContextEntity contextEntity, IIdentifiable entity)
        {
            var includedEntities = GetIncludedEntities(includedObject, contextEntity, entity);
            if (includedEntities?.Count > 0)
            {
                includedObject = includedEntities;
            }

            return includedObject;
        }

        [Obsolete("You should specify an IResourceDefinition implementation using the GetData/3 overload.")]
        public ResourceObject GetData(ContextEntity contextEntity, IIdentifiable entity)
            => GetData(contextEntity, entity, resourceDefinition: null);

        /// <inheritdoc />
        public ResourceObject GetData(ContextEntity contextEntity, IIdentifiable entity, IResourceDefinition resourceDefinition = null)
        {
            var data = new ResourceObject
            {
                Type = contextEntity.EntityName,
                Id = entity.StringId
            };

            if (_jsonApiContext.IsRelationshipPath)
                return data;

            data.Attributes = new Dictionary<string, object>();

            var resourceAttributes = resourceDefinition?.GetOutputAttrs(entity) ?? contextEntity.Attributes;
            resourceAttributes.ForEach(attr =>
            {
                var attributeValue = attr.GetValue(entity);
                if (ShouldIncludeAttribute(attr, attributeValue))
                {
                    data.Attributes.Add(attr.PublicAttributeName, attributeValue);
                }
            });

            if (contextEntity.Relationships.Count > 0)
                AddRelationships(data, contextEntity, entity);

            return data;
        }
        private bool ShouldIncludeAttribute(AttrAttribute attr, object attributeValue, RelationshipAttribute relationship = null)
        {
            return OmitNullValuedAttribute(attr, attributeValue) == false
                    && attr.InternalAttributeName != nameof(Identifiable.Id)
                   && ((_jsonApiContext.QuerySet == null
                       || _jsonApiContext.QuerySet.Fields.Count == 0)
                       || _jsonApiContext.QuerySet.Fields.Contains(relationship != null ?
                            $"{relationship.InternalRelationshipName}.{attr.InternalAttributeName}" :
                            attr.InternalAttributeName));
        }

        private bool OmitNullValuedAttribute(AttrAttribute attr, object attributeValue)
        {
            return attributeValue == null && _documentBuilderOptions.OmitNullValuedAttributes;
        }

        private void AddRelationships(ResourceObject data, ContextEntity contextEntity, IIdentifiable entity)
        {
            data.Relationships = new Dictionary<string, RelationshipData>();
            contextEntity.Relationships.ForEach(r =>
                data.Relationships.Add(
                    r.PublicRelationshipName,
                    GetRelationshipData(r, contextEntity, entity)
                )
            );
        }

        private RelationshipData GetRelationshipData(RelationshipAttribute attr, ContextEntity contextEntity, IIdentifiable entity)
        {
            var linkBuilder = new LinkBuilder(_jsonApiContext);

            var relationshipData = new RelationshipData();

            if (_jsonApiContext.Options.DefaultRelationshipLinks.HasFlag(Link.None) == false && attr.DocumentLinks.HasFlag(Link.None) == false)
            {
                relationshipData.Links = new Links();
                if (attr.DocumentLinks.HasFlag(Link.Self))
                    relationshipData.Links.Self = linkBuilder.GetSelfRelationLink(contextEntity.EntityName, entity.StringId, attr.PublicRelationshipName);

                if (attr.DocumentLinks.HasFlag(Link.Related))
                    relationshipData.Links.Related = linkBuilder.GetRelatedRelationLink(contextEntity.EntityName, entity.StringId, attr.PublicRelationshipName);
            }

            // this only includes the navigation property, we need to actually check the navigation property Id
            var navigationEntity = _jsonApiContext.ResourceGraph.GetRelationshipValue(entity, attr);
            if (navigationEntity == null)
                relationshipData.SingleData = attr.IsHasOne
                    ? GetIndependentRelationshipIdentifier((HasOneAttribute)attr, entity)
                    : null;
            else if (navigationEntity is IEnumerable)
                relationshipData.ManyData = GetRelationships((IEnumerable<object>)navigationEntity);
            else
                relationshipData.SingleData = GetRelationship(navigationEntity);

            return relationshipData;
        }

        private List<ResourceObject> GetIncludedEntities(List<ResourceObject> included, ContextEntity rootContextEntity, IIdentifiable rootResource)
        {
            if (_jsonApiContext.IncludedRelationships != null)
            {
                foreach (var relationshipName in _jsonApiContext.IncludedRelationships)
                {
                    var relationshipChain = relationshipName.Split('.');

                    var contextEntity = rootContextEntity;
                    var entity = rootResource;
                    included = IncludeRelationshipChain(included, rootContextEntity, rootResource, relationshipChain, 0);
                }
            }

            return included;
        }

        private List<ResourceObject> IncludeRelationshipChain(
            List<ResourceObject> included, ContextEntity parentEntity, IIdentifiable parentResource, string[] relationshipChain, int relationshipChainIndex)
        {
            var requestedRelationship = relationshipChain[relationshipChainIndex];
            var relationship = parentEntity.Relationships.FirstOrDefault(r => r.PublicRelationshipName == requestedRelationship);
            if(relationship == null)
                throw new JsonApiException(400, $"{parentEntity.EntityName} does not contain relationship {requestedRelationship}");

            var navigationEntity = _jsonApiContext.ResourceGraph.GetRelationshipValue(parentResource, relationship);
            if(navigationEntity == null)
                return included;
            if (navigationEntity is IEnumerable hasManyNavigationEntity)
            {
                foreach (IIdentifiable includedEntity in hasManyNavigationEntity)
                {
                    included = AddIncludedEntity(included, includedEntity, relationship);
                    included = IncludeSingleResourceRelationships(included, includedEntity, relationship, relationshipChain, relationshipChainIndex);
                }
            }
            else
            {
                included = AddIncludedEntity(included, (IIdentifiable)navigationEntity, relationship);
                included = IncludeSingleResourceRelationships(included, (IIdentifiable)navigationEntity, relationship, relationshipChain, relationshipChainIndex);
            }

            return included;
        }

        private List<ResourceObject> IncludeSingleResourceRelationships(
            List<ResourceObject> included, IIdentifiable navigationEntity, RelationshipAttribute relationship, string[] relationshipChain, int relationshipChainIndex)
        {
            if (relationshipChainIndex < relationshipChain.Length)
            {
                var nextContextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(relationship.DependentType);
                var resource = (IIdentifiable)navigationEntity;
                // recursive call
                if (relationshipChainIndex < relationshipChain.Length - 1)
                    included = IncludeRelationshipChain(included, nextContextEntity, resource, relationshipChain, relationshipChainIndex + 1);
            }

            return included;
        }


        private List<ResourceObject> AddIncludedEntity(List<ResourceObject> entities, IIdentifiable entity, RelationshipAttribute relationship)
        {
            var includedEntity = GetIncludedEntity(entity, relationship);

            if (entities == null)
                entities = new List<ResourceObject>();

            if (includedEntity != null && entities.Any(doc =>
                string.Equals(doc.Id, includedEntity.Id) && string.Equals(doc.Type, includedEntity.Type)) == false)
            {
                entities.Add(includedEntity);
            }

            return entities;
        }

        private ResourceObject GetIncludedEntity(IIdentifiable entity, RelationshipAttribute relationship)
        {
            if (entity == null) return null;

            var contextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(entity.GetType());
            var resourceDefinition = _scopedServiceProvider.GetService(contextEntity.ResourceType) as IResourceDefinition;

            var data = GetData(contextEntity, entity, resourceDefinition);

            data.Attributes = new Dictionary<string, object>();

            contextEntity.Attributes.ForEach(attr =>
            {
                var attributeValue = attr.GetValue(entity);
                if (ShouldIncludeAttribute(attr, attributeValue, relationship))
                {
                    data.Attributes.Add(attr.PublicAttributeName, attributeValue);
                }
            });

            return data;
        }

        private List<ResourceIdentifierObject> GetRelationships(IEnumerable<object> entities)
        {
            string typeName = null;
            var relationships = new List<ResourceIdentifierObject>();
            foreach (var entity in entities)
            {
                // this method makes the assumption that entities is a homogenous collection
                // so, we just lookup the type of the first entity on the graph
                // this is better than trying to get it from the generic parameter since it could
                // be less specific than what is registered on the graph (e.g. IEnumerable<object>) 
                typeName = typeName ?? _jsonApiContext.ResourceGraph.GetContextEntity(entity.GetType()).EntityName;
                relationships.Add(new ResourceIdentifierObject
                {
                    Type = typeName,
                    Id = ((IIdentifiable)entity).StringId
                });
            }
            return relationships;
        }

        private ResourceIdentifierObject GetRelationship(object entity)
        {
            var objType = entity.GetType();
            var contextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(objType);

            if (entity is IIdentifiable identifiableEntity)
                return new ResourceIdentifierObject
                {
                    Type = contextEntity.EntityName,
                    Id = identifiableEntity.StringId
                };

            return null;
        }

        private ResourceIdentifierObject GetIndependentRelationshipIdentifier(HasOneAttribute hasOne, IIdentifiable entity)
        {
            var independentRelationshipIdentifier = hasOne.GetIdentifiablePropertyValue(entity);
            if (independentRelationshipIdentifier == null)
                return null;

            var relatedContextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(hasOne.DependentType);
            if (relatedContextEntity == null) // TODO: this should probably be a debug log at minimum
                return null;

            return new ResourceIdentifierObject
            {
                Type = relatedContextEntity.EntityName,
                Id = independentRelationshipIdentifier.ToString()
            };
        }
    }
}
