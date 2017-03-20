using System;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Formatters
{
    public class JsonApiWriter : IJsonApiWriter
    {
        private readonly ILogger<JsonApiWriter> _logger;
        private readonly IJsonApiSerializer _serializer;

        public JsonApiWriter(IJsonApiSerializer serializer, 
            ILoggerFactory loggerFactory)
        {
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
            return _serializer.Serialize(responseObject);
        }        
    }
}