using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public static class JsonApiSerializer
    {
        public static string Serialize(object entity, IContextGraph contextGraph)
        {
            if (entity is IEnumerable<IIdentifiable>)
                return _serializeDocuments(entity, contextGraph);
            return _serializeDocument(entity, contextGraph);           
        }

        private static string _serializeDocuments(object entity, IContextGraph contextGraph)
        {
            var documentBuilder = new DocumentBuilder(contextGraph);
            var entities = entity as IEnumerable<IIdentifiable>;
            var documents = documentBuilder.Build(entities);
            return JsonConvert.SerializeObject(documents);
        }

         private static string _serializeDocument(object entity, IContextGraph contextGraph)
        {
            var documentBuilder = new DocumentBuilder(contextGraph);
            var identifiableEntity = entity as IIdentifiable;
            var document = documentBuilder.Build(identifiableEntity);
            return JsonConvert.SerializeObject(document);
        }
    }
}
