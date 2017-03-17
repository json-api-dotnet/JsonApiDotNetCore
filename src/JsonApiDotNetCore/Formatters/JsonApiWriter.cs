using System;
using System.Text;
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
    public class JsonApiWriter : IJsonApiWriter
    {
        private readonly ILogger<JsonApiWriter> _logger;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IJsonApiSerializer _serializer;

        public JsonApiWriter(IJsonApiContext jsonApiContext, 
            IJsonApiSerializer serializer, 
            ILoggerFactory loggerFactory)
        {
            _jsonApiContext = jsonApiContext;
            _serializer = serializer;
            _logger = loggerFactory.CreateLogger<JsonApiWriter>();
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _logger?.LogInformation("Formatting response as JSONAPI");

            var response = context.HttpContext.Response;
            using (var writer = context.WriterFactory(response.Body, Encoding.UTF8))
            {
                response.ContentType = "application/vnd.api+json";
                string responseContent;
                try
                {
                    responseContent = GetResponseBody(context.Object);
                }
                catch (Exception e)
                {
                    _logger?.LogError(new EventId(), e, "An error ocurred while formatting the response");
                    var errors = new ErrorCollection();
                    errors.Add(new Error("400", e.Message));
                    responseContent = errors.GetJson();
                    response.StatusCode = 400;
                }

                await writer.WriteAsync(responseContent);
                await writer.FlushAsync();
            }
        }

        private string GetResponseBody(object responseObject)
        {
            if (responseObject == null)
                return GetNullDataResponse();

            if (responseObject.GetType() == typeof(Error) || _jsonApiContext.RequestEntity == null)
                return GetErrorJson(responseObject, _logger);
            
            return _serializer.Serialize(responseObject);
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
            if (responseObject.GetType() == typeof(Error))
            {
                var errors = new ErrorCollection();
                errors.Add((Error)responseObject);
                return errors.GetJson();
            }
            else
            {
                logger?.LogInformation("Response was not a JSONAPI entity. Serializing as plain JSON.");
                return JsonConvert.SerializeObject(responseObject);
            }
        }
    }
}