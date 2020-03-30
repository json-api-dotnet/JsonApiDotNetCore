using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCore.Serialization.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    /// <summary>
    /// Formats the response data used  https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0.
    /// It was intended to have as little dependencies as possible in formatting layer for greater extensibility.
    /// It only depends on <see cref="IJsonApiSerializer"/>.
    /// </summary>
    public class JsonApiWriter : IJsonApiWriter
    {
        private readonly ILogger<JsonApiWriter> _logger;
        private readonly IJsonApiSerializer _serializer;

        public JsonApiWriter(IJsonApiSerializer serializer,
                             ILoggerFactory loggerFactory)
        {
            _serializer = serializer;
            _logger = loggerFactory.CreateLogger<JsonApiWriter>();

            _logger.LogTrace("Executing constructor.");
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;
            await using var writer = context.WriterFactory(response.Body, Encoding.UTF8);
            string responseContent;

            if (_serializer == null)
            {
                responseContent = JsonConvert.SerializeObject(context.Object);
            }
            else
            {
                response.ContentType = Constants.ContentType;
                try
                {
                    if (context.Object is ProblemDetails problemDetails)
                    {
                        var document = new ErrorDocument();
                        document.Errors.Add(ConvertProblemDetailsToError(problemDetails));
                        responseContent = _serializer.Serialize(document);
                    } else
                    {
                        responseContent = _serializer.Serialize(context.Object);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(new EventId(), e, "An error occurred while formatting the response");
                    var document = new ErrorDocument();
                    document.Errors.Add(new Error(HttpStatusCode.InternalServerError, e.Message, ErrorMeta.FromException(e)));
                    responseContent = _serializer.Serialize(document);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            await writer.WriteAsync(responseContent);
            await writer.FlushAsync();
        }

        private static Error ConvertProblemDetailsToError(ProblemDetails problemDetails)
        {
            return new Error
            {
                Id = !string.IsNullOrWhiteSpace(problemDetails.Instance)
                    ? problemDetails.Instance
                    : Guid.NewGuid().ToString(),
                Links = !string.IsNullOrWhiteSpace(problemDetails.Type)
                    ? new ErrorLinks {About = problemDetails.Type}
                    : null,
                Status = problemDetails.Status != null
                    ? (HttpStatusCode)problemDetails.Status.Value
                    : HttpStatusCode.InternalServerError,
                Title = problemDetails.Title,
                Detail = problemDetails.Detail
            };
        }
    }
}
