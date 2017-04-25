using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Builders
{
    public class DocumentBuilder : IDocumentBuilder
    {
        private IJsonApiContext _jsonApiContext;
        private IContextGraph _contextGraph;
        private readonly IRequestMeta _requestMeta;

        public DocumentBuilder(IJsonApiContext jsonApiContext)
        {
            _jsonApiContext = jsonApiContext;
            _contextGraph = jsonApiContext.ContextGraph;
        }

        public DocumentBuilder(IJsonApiContext jsonApiContext, IRequestMeta requestMeta)
        {
            _jsonApiContext = jsonApiContext;
            _contextGraph = jsonApiContext.ContextGraph;
            _requestMeta = requestMeta;
        }

        public Document Build(IIdentifiable entity)
        {
            var contextEntity = _contextGraph.GetContextEntity(entity.GetType());

            var document = new Document
            {
                Data = GetData(contextEntity, entity),
                Meta = GetMeta(entity),
                Links = _jsonApiContext.PageManager.GetPageLinks(new LinkBuilder(_jsonApiContext))
            };

            document.Included = AppendIncludedObject(document.Included, contextEntity, entity);

            return document;
        }

        public Documents Build(IEnumerable<IIdentifiable> entities)
        {
            var entityType = entities
                .GetType()
                .GenericTypeArguments[0];

            var contextEntity = _contextGraph.GetContextEntity(entityType);

            var documents = new Documents
            {
                Data = new List<DocumentData>(),
                Meta = GetMeta(entities.FirstOrDefault()),
                Links = _jsonApiContext.PageManager.GetPageLinks(new LinkBuilder(_jsonApiContext))
            };

            foreach (var entity in entities)
            {
                documents.Data.Add(GetData(contextEntity, entity));
                documents.Included = AppendIncludedObject(documents.Included, contextEntity, entity);
            }

            return documents;
        }

        private Dictionary<string, object> GetMeta(IIdentifiable entity)
        {
            if (entity == null) return null;
            
            var builder = _jsonApiContext.MetaBuilder;

            if(entity is IHasMeta metaEntity)
                builder.Add(metaEntity.GetMeta(_jsonApiContext));

            if(_jsonApiContext.Options.IncludeTotalRecordCount)
                builder.Add("total-records", _jsonApiContext.PageManager.TotalRecords);
            
            if(_requestMeta != null)
                builder.Add(_requestMeta.GetMeta());

            var meta = builder.Build();
            if(meta.Count > 0) return meta;
            return null;
        }

        private List<DocumentData> AppendIncludedObject(List<DocumentData> includedObject, ContextEntity contextEntity, IIdentifiable entity)
        {
            var includedEntities = GetIncludedEntities(contextEntity, entity);
            if (includedEntities.Count > 0)
            {
                if (includedObject == null)
                    includedObject = new List<DocumentData>();
                includedObject.AddRange(includedEntities);
            }

            return includedObject;
        }

        private DocumentData GetData(ContextEntity contextEntity, IIdentifiable entity)
        {
            var data = new DocumentData
            {
                Type = contextEntity.EntityName,
                Id = entity.StringId
            };

            if (_jsonApiContext.IsRelationshipData)
                return data;

            data.Attributes = new Dictionary<string, object>();

            contextEntity.Attributes.ForEach(attr =>
            {
                if(ShouldIncludeAttribute(attr))
                    data.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
            });

            if (contextEntity.Relationships.Count > 0)
                AddRelationships(data, contextEntity, entity);

            return data;
        }

        private bool ShouldIncludeAttribute(AttrAttribute attr)
        {
            return (_jsonApiContext.QuerySet == null 
                || _jsonApiContext.QuerySet.Fields.Count == 0 
                || _jsonApiContext.QuerySet.Fields.Contains(attr.InternalAttributeName));
        }

        private void AddRelationships(DocumentData data, ContextEntity contextEntity, IIdentifiable entity)
        {
            data.Relationships = new Dictionary<string, RelationshipData>();
            var linkBuilder = new LinkBuilder(_jsonApiContext);

            contextEntity.Relationships.ForEach(r =>
            {
                var relationshipData = new RelationshipData
                {
                    Links = new Links
                    {
                        Self = linkBuilder.GetSelfRelationLink(contextEntity.EntityName, entity.StringId, r.PublicRelationshipName),
                        Related = linkBuilder.GetRelatedRelationLink(contextEntity.EntityName, entity.StringId, r.PublicRelationshipName)
                    }
                };

                if (RelationshipIsIncluded(r.PublicRelationshipName))
                {
                    var navigationEntity = _jsonApiContext.ContextGraph
                        .GetRelationship(entity, r.InternalRelationshipName);

                    if(navigationEntity == null)
                        relationshipData.SingleData = null;
                    else if (navigationEntity is IEnumerable)
                        relationshipData.ManyData = GetRelationships((IEnumerable<object>)navigationEntity, r.InternalRelationshipName);
                    else
                        relationshipData.SingleData = GetRelationship(navigationEntity, r.InternalRelationshipName);
                }

                data.Relationships.Add(r.PublicRelationshipName, relationshipData);
            });
        }

        private List<DocumentData> GetIncludedEntities(ContextEntity contextEntity, IIdentifiable entity)
        {
            var included = new List<DocumentData>();

            contextEntity.Relationships.ForEach(r =>
            {
                if (!RelationshipIsIncluded(r.PublicRelationshipName)) return;

                var navigationEntity = _jsonApiContext.ContextGraph.GetRelationship(entity, r.InternalRelationshipName);

                if (navigationEntity is IEnumerable)
                    foreach (var includedEntity in (IEnumerable)navigationEntity)
                        AddIncludedEntity(included, (IIdentifiable)includedEntity);
                else
                    AddIncludedEntity(included, (IIdentifiable)navigationEntity);
            });

            return included;
        }

        private void AddIncludedEntity(List<DocumentData> entities, IIdentifiable entity)
        {
            var includedEntity = GetIncludedEntity(entity);

            if(includedEntity != null && !entities.Any(doc => doc.Id == includedEntity.Id && doc.Type == includedEntity.Type))
                entities.Add(includedEntity);
        }

        private DocumentData GetIncludedEntity(IIdentifiable entity)
        {
            if(entity == null) return null;
            
            var contextEntity = _jsonApiContext.ContextGraph.GetContextEntity(entity.GetType());

            var data = GetData(contextEntity, entity);

            data.Attributes = new Dictionary<string, object>();

            contextEntity.Attributes.ForEach(attr =>
            {
                data.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
            });

            return data;
        }

        private bool RelationshipIsIncluded(string relationshipName)
        {
            return _jsonApiContext.IncludedRelationships != null &&
                _jsonApiContext.IncludedRelationships.Contains(relationshipName);
        }

        private List<Dictionary<string, string>> GetRelationships(IEnumerable<object> entities, string relationshipName)
        {
            var objType = entities.GetType().GenericTypeArguments[0];

            var typeName = _jsonApiContext.ContextGraph.GetContextEntity(objType);

            var relationships = new List<Dictionary<string, string>>();
            foreach (var entity in entities)
            {
                relationships.Add(new Dictionary<string, string> {
                    {"type", typeName.EntityName },
                    {"id", ((IIdentifiable)entity).StringId }
                });
            }
            return relationships;
        }
        private Dictionary<string, string> GetRelationship(object entity, string relationshipName)
        {
            var objType = entity.GetType();

            var typeName = _jsonApiContext.ContextGraph.GetContextEntity(objType);

            return new Dictionary<string, string> {
                    {"type", typeName.EntityName },
                    {"id", ((IIdentifiable)entity).StringId }
                };
        }
    }
}
