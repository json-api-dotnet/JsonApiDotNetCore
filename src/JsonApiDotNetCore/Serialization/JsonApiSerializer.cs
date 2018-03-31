using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    public class JsonApiSerializer : IJsonApiSerializer
    {
        private readonly IDocumentBuilder _documentBuilder;
        private readonly ILogger<JsonApiSerializer> _logger;
        private readonly IJsonApiContext _jsonApiContext;

        public JsonApiSerializer(
            IJsonApiContext jsonApiContext,
            IDocumentBuilder documentBuilder)
        {
            _jsonApiContext = jsonApiContext;
            _documentBuilder = documentBuilder;
        }

        public JsonApiSerializer(
            IJsonApiContext jsonApiContext,
            IDocumentBuilder documentBuilder,
            ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext;
            _documentBuilder = documentBuilder;
            _logger = loggerFactory?.CreateLogger<JsonApiSerializer>();
        }

        public string Serialize(object entity)
        {
            if (entity == null)
                return GetNullDataResponse();

            if (entity.GetType() == typeof(ErrorCollection) || (_jsonApiContext.RequestEntity == null && _jsonApiContext.IsBulkOperationRequest == false))
                return GetErrorJson(entity, _logger);

            if (_jsonApiContext.IsBulkOperationRequest)
                return _serialize(entity);

            if (entity is IEnumerable<IIdentifiable>)
                return SerializeDocuments(entity);

            return SerializeDocument(entity);
        }

        private string GetNullDataResponse()
        {
            return JsonConvert.SerializeObject(new Document
            {
                Data = null
            });
        }

        private string GetErrorJson(object responseObject, ILogger logger)
        {
            if (responseObject is ErrorCollection errorCollection)
            {
                return errorCollection.GetJson();
            }
            else
            {
                logger?.LogInformation("Response was not a JSONAPI entity. Serializing as plain JSON.");
                return JsonConvert.SerializeObject(responseObject);
            }
        }

        private string SerializeDocuments(object entity)
        {
            var entities = entity as IEnumerable<IIdentifiable>;
            var documents = _documentBuilder.Build(entities);
            return _serialize(documents);
        }

        private string SerializeDocument(object entity)
        {
            var identifiableEntity = entity as IIdentifiable;
            var document = _documentBuilder.Build(identifiableEntity);
            return _serialize(document);
        }

        private string _serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _jsonApiContext.Options.SerializerSettings);
        }
    }
}
