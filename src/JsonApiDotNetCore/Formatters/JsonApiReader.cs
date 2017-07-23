using System;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiReader : IJsonApiReader
    {
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly ILogger<JsonApiReader> _logger;


        public JsonApiReader(IJsonApiDeSerializer deSerializer, IJsonApiContext jsonApiContext, ILoggerFactory loggerFactory)
        {
            _deSerializer = deSerializer;
            _jsonApiContext = jsonApiContext;
            _logger = loggerFactory.CreateLogger<JsonApiReader>();
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
                return InputFormatterResult.SuccessAsync(null);

            try
            {
                var body = GetRequestBody(context.HttpContext.Request.Body);
                var model = _jsonApiContext.IsRelationshipPath ?
                    _deSerializer.DeserializeRelationship(body) :
                    _deSerializer.Deserialize(body);

                if (model == null)
                    _logger?.LogError("An error occurred while de-serializing the payload");

                return InputFormatterResult.SuccessAsync(model);
            }
            catch (JsonSerializationException ex)
            {
                _logger?.LogError(new EventId(), ex, "An error occurred while de-serializing the payload");
                context.ModelState.AddModelError(context.ModelName, ex, context.Metadata);
                return InputFormatterResult.FailureAsync();
            }
            catch (JsonApiException jex)
            {
                _logger?.LogError(new EventId(), jex, "An error occurred while de-serializing the payload");
                context.ModelState.AddModelError(context.ModelName, jex, context.Metadata);
                return InputFormatterResult.FailureAsync();
            }
        }

        private string GetRequestBody(Stream body)
        {
            using (var reader = new StreamReader(body))
            {
                return reader.ReadToEnd();
            }
        }
    }
}