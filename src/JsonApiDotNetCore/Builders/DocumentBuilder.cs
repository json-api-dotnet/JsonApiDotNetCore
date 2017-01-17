using System.Collections.Generic;
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

            if(_jsonApiContext.IsRelationshipData)
                return data;

            data.Attributes = new Dictionary<string, object>();
            data.Relationships = new Dictionary<string, Dictionary<string, object>>();

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

            contextEntity.Relationships.ForEach(r => {
                data.Relationships.Add("links", new Dictionary<string,object> {
                    {"self", linkBuilder.GetSelfRelationLink(contextEntity.EntityName, entity.Id.ToString(), r.RelationshipName)},
                    {"related", linkBuilder.GetRelatedRelationLink(contextEntity.EntityName, entity.Id.ToString(), r.RelationshipName)},
                });
            });
        }
    }
}
