using System;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
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
                
                string responseContent;
                try
                {
                    if(context.Object.GetType() == typeof(Error) || jsonApiContext.RequestEntity == null)
                    {
                        logger?.LogInformation("Response was not a JSONAPI entity. Serializing as plain JSON.");
                        responseContent = JsonConvert.SerializeObject(context.Object);
                    }
                    else
                    {
                        response.ContentType = "application/vnd.api+json";
                        responseContent = JsonApiSerializer.Serialize(context.Object, jsonApiContext);
                    }
                }
                catch(Exception e)
                {
                    logger?.LogError(new EventId(), e, "An error ocurred while formatting the response");
                    responseContent = new Error("400", e.Message).GetJson();
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
    }
}
