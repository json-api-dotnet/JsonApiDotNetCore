using System;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiOutputFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var contentTypeString = context.HttpContext.Request.ContentType;

            return string.IsNullOrEmpty(contentTypeString) || contentTypeString == "application/vnd.api+json";
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var logger = GetService<ILoggerFactory>(context)?
                .CreateLogger<JsonApiOutputFormatter>();

            logger?.LogInformation("Formatting response as JSONAPI");

            var response = context.HttpContext.Response;
            using (var writer = context.WriterFactory(response.Body, Encoding.UTF8))
            {
                var jsonApiContext = GetService<IJsonApiContext>(context);

                response.ContentType = "application/vnd.api+json";
                string responseContent;
                try
                {
                    responseContent = GetResponseBody(context.Object, jsonApiContext, logger);
                }
                catch (Exception e)
                {
                    logger?.LogError(new EventId(), e, "An error ocurred while formatting the response");
                    var errors = new ErrorCollection();
                    errors.Add(new Error("400", e.Message));
                    responseContent = errors.GetJson();
                    response.StatusCode = 400;
                }

                await writer.WriteAsync(responseContent);
                await writer.FlushAsync();
            }
        }

        private T GetService<T>(OutputFormatterWriteContext context)
        {
            return context.HttpContext.RequestServices.GetService<T>();
        }

        private string GetResponseBody(object responseObject, IJsonApiContext jsonApiContext, ILogger logger)
        {
            if (responseObject == null)
                return GetNullDataResponse();

            if (responseObject.GetType() == typeof(Error) || jsonApiContext.RequestEntity == null)
                return GetErrorJson(responseObject, logger);
            
            return JsonApiSerializer.Serialize(responseObject, jsonApiContext);
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
