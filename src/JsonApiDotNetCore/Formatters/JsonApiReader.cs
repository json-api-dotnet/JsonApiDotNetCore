using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    /// <inheritdoc />
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

                object model = null;

                if (_jsonApiContext.IsRelationshipPath)
                {
                    model = _deSerializer.DeserializeRelationship(body);
                }
                else
                {
                    model = _deSerializer.Deserialize(body);
                }


                if (model == null)
                {
                    _logger?.LogError("An error occurred while de-serializing the payload");
                }

                if (context.HttpContext.Request.Method == "PATCH")
                {
                    bool idMissing;
                    if (model is IList list)
                    {
                        idMissing = CheckForId(list);
                    }
                    else
                    {
                        idMissing = CheckForId(model);
                    }
                    if (idMissing)
                    {
                        _logger?.LogError("Payload must include id attribute");
                        throw new JsonApiException(400, "Payload must include id attribute");
                    }
                }
                return InputFormatterResult.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                _logger?.LogError(new EventId(), ex, "An error occurred while de-serializing the payload");
                context.ModelState.AddModelError(context.ModelName, ex, context.Metadata);
                return InputFormatterResult.FailureAsync();
            }
        }

        private bool CheckForId(object model)
        {
            if (model == null) return false;
            if (model is ResourceObject ro)
            {
                if (string.IsNullOrEmpty(ro.Id)) return true;
            }
            else if (model is IIdentifiable identifiable)
            {
                if (string.IsNullOrEmpty(identifiable.StringId)) return true;
            }
            return false;
        }

        private bool CheckForId(IList modelList)
        {
            foreach (var model in modelList)
            {
                if (model == null) continue;
                if (model is ResourceObject ro)
                {
                    if (string.IsNullOrEmpty(ro.Id)) return true;
                }
                else if (model is IIdentifiable identifiable)
                {
                    if (string.IsNullOrEmpty(identifiable.StringId)) return true;
                }
            }
            return false;

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
