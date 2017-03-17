using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public class JsonApiSerializer : IJsonApiSerializer
    {
        private readonly IDocumentBuilder _documentBuilder;

        public JsonApiSerializer(IDocumentBuilder documentBuilder)
        {
            _documentBuilder = documentBuilder;
        }

        public string Serialize(object entity)
        {
            if (entity is IEnumerable<IIdentifiable>)
                return _serializeDocuments(entity);

            return _serializeDocument(entity);           
        }

        private string _serializeDocuments(object entity)
        {
            var entities = entity as IEnumerable<IIdentifiable>;
            var documents = _documentBuilder.Build(entities);
            return _serialize(documents);
        }

        private string _serializeDocument(object entity)
        {
            var identifiableEntity = entity as IIdentifiable;
            var document = _documentBuilder.Build(identifiableEntity);
            return _serialize(document);
        }

        private string _serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
