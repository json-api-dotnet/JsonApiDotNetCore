using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public class DocumentBuilder
    {
        private IContextGraph _contextGraph;

        public DocumentBuilder(IContextGraph contextGraph)
        {
            _contextGraph = contextGraph;
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
                Id = entity.Id.ToString(),
                Attributes = new Dictionary<string, object>()
            };

            contextEntity.Attributes.ForEach(attr =>
            {
                data.Attributes.Add(attr.PublicAttributeName, attr.GetValue(entity));
            });

            return data;
        }
    }
}
