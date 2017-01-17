using System;
using System.Collections;
using System.Collections.Generic;
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
                Data = _getData(contextEntity, entity)
            };

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
                Data = new List<DocumentData>()
            };

            foreach (var entity in entities)
                documents.Data.Add(_getData(contextEntity, entity));

            return documents;      
        }

        private DocumentData _getData(ContextEntity contextEntity, IIdentifiable entity)
        {
            var data = new DocumentData
            {
                Type = contextEntity.EntityName,
                Id = entity.Id.ToString()
            };

            if (_jsonApiContext.IsRelationshipData)
                return data;

            data.Attributes = new Dictionary<string, object>();
            data.Relationships = new Dictionary<string, RelationshipData>();

            contextEntity.Attributes.ForEach(attr =>
            {
                data.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
            });

            _addRelationships(data, contextEntity, entity);

            return data;
        }

        private void _addRelationships(DocumentData data, ContextEntity contextEntity, IIdentifiable entity)
        {
            var linkBuilder = new LinkBuilder(_jsonApiContext);

            contextEntity.Relationships.ForEach(r =>
            {
                var relationshipData = new RelationshipData
                {
                    Links = new Links
                    {
                        Self = linkBuilder.GetSelfRelationLink(contextEntity.EntityName, entity.Id.ToString(), r.RelationshipName),
                        Related = linkBuilder.GetRelatedRelationLink(contextEntity.EntityName, entity.Id.ToString(), r.RelationshipName)
                    }
                };

                if (_hasRelationship(r.RelationshipName))
                {
                    var navigationEntity = _jsonApiContext.ContextGraph
                        .GetRelationship(entity, r.RelationshipName);

                    if(navigationEntity is IEnumerable)
                        relationshipData.ManyData = GetRelationships((IEnumerable<object>)navigationEntity, r.RelationshipName);
                    else
                        relationshipData.SingleData = GetRelationship(navigationEntity, r.RelationshipName);
                }
                    


                data.Relationships.Add(r.RelationshipName.Dasherize(), relationshipData);
            });
        }

        private bool _hasRelationship(string relationshipName)
        {
            return _jsonApiContext.IncludedRelationships != null && 
                _jsonApiContext.IncludedRelationships.Contains(relationshipName.ToProperCase());
        }

        private List<Dictionary<string, string>> GetRelationships(IEnumerable<object> entities, string relationshipName)
        {
            var objType = entities.GetType().GenericTypeArguments[0];
            
            var typeName = _jsonApiContext.ContextGraph.GetContextEntity(objType);

            var relationships = new List<Dictionary<string, string>>();
            foreach(var entity in entities)
            {
                relationships.Add(new Dictionary<string, string> {
                    {"type", typeName.EntityName },
                    {"id", ((IIdentifiable)entity).Id.ToString() }
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
                    {"id", ((IIdentifiable)entity).Id.ToString() }
                };
        }
    }
}
