using System;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

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

            var response = context.HttpContext.Response;

            using (var writer = context.WriterFactory(response.Body, Encoding.UTF8))
            {
                var jsonApiContext = context.HttpContext.RequestServices.GetService<IJsonApiContext>();

                var responseContent = JsonApiSerializer.Serialize(context.Object, jsonApiContext);

                await writer.WriteAsync(responseContent);

                await writer.FlushAsync();
            }
        }
    }
}
