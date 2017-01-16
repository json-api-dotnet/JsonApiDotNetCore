using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public static class JsonApiSerializer
    {
        public static string Serialize(object entity, IJsonApiContext jsonApiContext)
        {
            if (entity is IEnumerable<IIdentifiable>)
                return _serializeDocuments(entity, jsonApiContext);
            return _serializeDocument(entity, jsonApiContext);           
        }

        private static string _serializeDocuments(object entity, IJsonApiContext jsonApiContext)
        {
            var documentBuilder = new DocumentBuilder(jsonApiContext);

            var entities = entity as IEnumerable<IIdentifiable>;
            var documents = documentBuilder.Build(entities);
            return JsonConvert.SerializeObject(documents);
        }

         private static string _serializeDocument(object entity, IJsonApiContext jsonApiContext)
        {
            var documentBuilder = new DocumentBuilder(jsonApiContext);
            var identifiableEntity = entity as IIdentifiable;
            var document = documentBuilder.Build(identifiableEntity);
            return JsonConvert.SerializeObject(document);
        }
    }
}
