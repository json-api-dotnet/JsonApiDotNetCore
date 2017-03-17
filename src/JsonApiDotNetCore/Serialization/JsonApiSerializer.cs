using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public static class JsonApiSerializer
    {
        public static string Serialize(object entity, IJsonApiContext jsonApiContext, IRequestMeta requestMeta)
        {
            if (entity is IEnumerable<IIdentifiable>)
                return _serializeDocuments(entity, jsonApiContext, requestMeta);

            return _serializeDocument(entity, jsonApiContext, requestMeta);           
        }

        private static string _serializeDocuments(object entity, IJsonApiContext jsonApiContext, IRequestMeta requestMeta)
        {
            var documentBuilder = new DocumentBuilder(jsonApiContext, requestMeta);
            var entities = entity as IEnumerable<IIdentifiable>;
            var documents = documentBuilder.Build(entities);
            return _serialize(documents);
        }

        private static string _serializeDocument(object entity, IJsonApiContext jsonApiContext, IRequestMeta requestMeta)
        {
            var documentBuilder = new DocumentBuilder(jsonApiContext, requestMeta);
            var identifiableEntity = entity as IIdentifiable;
            var document = documentBuilder.Build(identifiableEntity);
            return _serialize(document);
        }

        private static string _serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
