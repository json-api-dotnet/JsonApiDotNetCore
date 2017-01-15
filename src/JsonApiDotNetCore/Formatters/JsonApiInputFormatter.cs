using System;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
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

            try
            {
                var body = GetRequestBody(context.HttpContext.Request.Body);
                var contextGraph = context.HttpContext.RequestServices.GetService<IJsonApiContext>().ContextGraph;
                var model = JsonApiDeSerializer.Deserialize(body, contextGraph);

                return InputFormatterResult.SuccessAsync(model);
            }
            catch (JsonSerializationException)
            {
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
    }
}
