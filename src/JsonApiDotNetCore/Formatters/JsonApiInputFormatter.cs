using System;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiInputFormatter : IInputFormatter
    {
        public bool CanRead(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var contentTypeString = context.HttpContext.Request.ContentType;

            return contentTypeString == "application/vnd.api+json";
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.HttpContext.Request;

            if (request.ContentLength == 0)
            {
                return InputFormatterResult.SuccessAsync(null);
            }

            var loggerFactory = GetService<ILoggerFactory>(context);
            var logger = loggerFactory?.CreateLogger<JsonApiInputFormatter>();

            try
            {
                var body = GetRequestBody(context.HttpContext.Request.Body);
                var jsonApiContext = GetService<IJsonApiContext>(context);
                var model = JsonApiDeSerializer.Deserialize(body, jsonApiContext);

                if(model == null)
                    logger?.LogError("An error occurred while de-serializing the payload");

                return InputFormatterResult.SuccessAsync(model);
            }
            catch (JsonSerializationException ex)
            {
                logger?.LogError(new EventId(), ex, "An error occurred while de-serializing the payload");
                context.HttpContext.Response.StatusCode = 422;
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

        private TService GetService<TService>(InputFormatterContext context)
        {
            return context.HttpContext.RequestServices.GetService<TService>();
        }
    }
}
