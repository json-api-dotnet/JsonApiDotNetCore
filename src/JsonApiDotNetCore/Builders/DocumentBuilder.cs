using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Builders
{
    public class DocumentBuilder
    {
        private IJsonApiContext _jsonApiContext;
        private IContextGraph _contextGraph;

        public DocumentBuilder(IJsonApiContext jsonApiContext)
        {
            _jsonApiContext = jsonApiContext;
            _contextGraph = jsonApiContext.ContextGraph;
        }

        public Document Build(IIdentifiable entity)
        {
            var contextEntity = _contextGraph.GetContextEntity(entity.GetType());

            var document = new Document
            {
                Data = _getData(contextEntity, entity),
                Meta = _getMeta(entity),
                Links = _jsonApiContext.PageManager.GetPageLinks(new LinkBuilder(_jsonApiContext))
            };

            document.Included = _appendIncludedObject(document.Included, contextEntity, entity);

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
                Meta = _getMeta(entities.FirstOrDefault()),
                Links = _jsonApiContext.PageManager.GetPageLinks(new LinkBuilder(_jsonApiContext))
            };

            foreach (var entity in entities)
            {
                documents.Data.Add(_getData(contextEntity, entity));
                documents.Included = _appendIncludedObject(documents.Included, contextEntity, entity);
            }

            return documents;
        }

        private Dictionary<string, object> _getMeta(IIdentifiable entity)
        {
            if (entity == null) return null;

            var meta = new Dictionary<string, object>();
            var metaEntity = entity as IHasMeta;
            
            if(metaEntity != null)
                meta = metaEntity.GetMeta(_jsonApiContext);

            if(_jsonApiContext.Options.IncludeTotalRecordCount)
                meta["total-records"] = _jsonApiContext.PageManager.TotalRecords;
            
            if(meta.Count > 0) return meta;
            return null;
        }

        private List<DocumentData> _appendIncludedObject(List<DocumentData> includedObject, ContextEntity contextEntity, IIdentifiable entity)
        {
            var includedEntities = _getIncludedEntities(contextEntity, entity);
            if (includedEntities.Count > 0)
            {
                if (includedObject == null)
                    includedObject = new List<DocumentData>();
                includedObject.AddRange(includedEntities);
            }

            return includedObject;
        }

        private DocumentData _getData(ContextEntity contextEntity, IIdentifiable entity)
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
                data.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
            });

            if (contextEntity.Relationships.Count > 0)
                _addRelationships(data, contextEntity, entity);

            return data;
        }

        private void _addRelationships(DocumentData data, ContextEntity contextEntity, IIdentifiable entity)
        {
            data.Relationships = new Dictionary<string, RelationshipData>();
            var linkBuilder = new LinkBuilder(_jsonApiContext);

            contextEntity.Relationships.ForEach(r =>
            {
                var relationshipData = new RelationshipData
                {
                    Links = new Links
                    {
                        Self = linkBuilder.GetSelfRelationLink(contextEntity.EntityName, entity.StringId, r.InternalRelationshipName),
                        Related = linkBuilder.GetRelatedRelationLink(contextEntity.EntityName, entity.StringId, r.InternalRelationshipName)
                    }
                };

                if (_relationshipIsIncluded(r.InternalRelationshipName))
                {
                    var navigationEntity = _jsonApiContext.ContextGraph
                        .GetRelationship(entity, r.InternalRelationshipName);

                    if(navigationEntity == null)
                        relationshipData.SingleData = null;
                    else if (navigationEntity is IEnumerable)
                        relationshipData.ManyData = _getRelationships((IEnumerable<object>)navigationEntity, r.InternalRelationshipName);
                    else
                        relationshipData.SingleData = _getRelationship(navigationEntity, r.InternalRelationshipName);
                }

                data.Relationships.Add(r.InternalRelationshipName.Dasherize(), relationshipData);
            });
        }

        private List<DocumentData> _getIncludedEntities(ContextEntity contextEntity, IIdentifiable entity)
        {
            var included = new List<DocumentData>();

            contextEntity.Relationships.ForEach(r =>
            {
                if (!_relationshipIsIncluded(r.InternalRelationshipName)) return;

                var navigationEntity = _jsonApiContext.ContextGraph.GetRelationship(entity, r.InternalRelationshipName);

                if (navigationEntity is IEnumerable)
                    foreach (var includedEntity in (IEnumerable)navigationEntity)
                        included.Add(_getIncludedEntity((IIdentifiable)includedEntity));
                else
                    included.Add(_getIncludedEntity((IIdentifiable)navigationEntity));
            });

            return included;
        }

        private DocumentData _getIncludedEntity(IIdentifiable entity)
        {
            if(entity == null) return null;
            
            var contextEntity = _jsonApiContext.ContextGraph.GetContextEntity(entity.GetType());

            var data = new DocumentData
            {
                Type = contextEntity.EntityName,
                Id = entity.StringId
            };

            data.Attributes = new Dictionary<string, object>();

            contextEntity.Attributes.ForEach(attr =>
            {
                data.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
            });

            return data;
        }

        private bool _relationshipIsIncluded(string relationshipName)
        {
            return _jsonApiContext.IncludedRelationships != null &&
                _jsonApiContext.IncludedRelationships.Contains(relationshipName.ToProperCase());
        }

        private List<Dictionary<string, string>> _getRelationships(IEnumerable<object> entities, string relationshipName)
        {
            var objType = entities.GetType().GenericTypeArguments[0];

            var typeName = _jsonApiContext.ContextGraph.GetContextEntity(objType);

            var relationships = new List<Dictionary<string, string>>();
            foreach (var entity in entities)
            {
                relationships.Add(new Dictionary<string, string> {
                    {"type", typeName.EntityName.Dasherize() },
                    {"id", ((IIdentifiable)entity).StringId }
                });
            }
            return relationships;
        }
        private Dictionary<string, string> _getRelationship(object entity, string relationshipName)
        {
            var objType = entity.GetType();

            var typeName = _jsonApiContext.ContextGraph.GetContextEntity(objType);

            return new Dictionary<string, string> {
                    {"type", typeName.EntityName.Dasherize() },
                    {"id", ((IIdentifiable)entity).StringId }
                };
        }
    }
}
