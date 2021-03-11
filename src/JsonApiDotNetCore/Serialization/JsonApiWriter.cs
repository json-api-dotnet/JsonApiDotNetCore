using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Formats the response data used (see https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0). It was intended to
    /// have as little dependencies as possible in formatting layer for greater extensibility.
    /// </summary>
    [PublicAPI]
    public class JsonApiWriter : IJsonApiWriter
    {
        private readonly IJsonApiSerializer _serializer;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly TraceLogWriter<JsonApiWriter> _traceWriter;

        public JsonApiWriter(IJsonApiSerializer serializer, IExceptionHandler exceptionHandler, ILoggerFactory loggerFactory)
        {
            ArgumentGuard.NotNull(serializer, nameof(serializer));
            ArgumentGuard.NotNull(exceptionHandler, nameof(exceptionHandler));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));

            _serializer = serializer;
            _exceptionHandler = exceptionHandler;
            _traceWriter = new TraceLogWriter<JsonApiWriter>(loggerFactory);
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            ArgumentGuard.NotNull(context, nameof(context));

            HttpResponse response = context.HttpContext.Response;
            response.ContentType = _serializer.ContentType;

            await using TextWriter writer = context.WriterFactory(response.Body, Encoding.UTF8);
            string responseContent;

            try
            {
                responseContent = SerializeResponse(context.Object, (HttpStatusCode)response.StatusCode);
            }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            {
                ErrorDocument errorDocument = _exceptionHandler.HandleException(exception);
                responseContent = _serializer.Serialize(errorDocument);

                response.StatusCode = (int)errorDocument.GetErrorStatusCode();
            }

            string url = context.HttpContext.Request.GetEncodedUrl();
            _traceWriter.LogMessage(() => $"Sending {response.StatusCode} response for request at '{url}' with body: <<{responseContent}>>");

            await writer.WriteAsync(responseContent);
            await writer.FlushAsync();
        }

        private string SerializeResponse(object contextObject, HttpStatusCode statusCode)
        {
            if (contextObject is ProblemDetails problemDetails)
            {
                throw new UnsuccessfulActionResultException(problemDetails);
            }

            if (contextObject == null)
            {
                if (!IsSuccessStatusCode(statusCode))
                {
                    throw new UnsuccessfulActionResultException(statusCode);
                }

                if (statusCode == HttpStatusCode.NoContent || statusCode == HttpStatusCode.ResetContent || statusCode == HttpStatusCode.NotModified)
                {
                    // Prevent exception from Kestrel server, caused by writing data:null json response.
                    return null;
                }
            }

            object contextObjectWrapped = WrapErrors(contextObject);

            return _serializer.Serialize(contextObjectWrapped);
        }

        private static object WrapErrors(object contextObject)
        {
            if (contextObject is IEnumerable<Error> errors)
            {
                return new ErrorDocument(errors);
            }

            if (contextObject is Error error)
            {
                return new ErrorDocument(error);
            }

            return contextObject;
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return new HttpResponseMessage(statusCode).IsSuccessStatusCode;
        }
    }
}
