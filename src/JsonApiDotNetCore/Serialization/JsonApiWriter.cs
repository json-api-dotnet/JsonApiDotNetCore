using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    public sealed class JsonApiWriter : IJsonApiWriter
    {
        private readonly IJsonApiSerializer _serializer;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IETagGenerator _eTagGenerator;
        private readonly TraceLogWriter<JsonApiWriter> _traceWriter;

        public JsonApiWriter(IJsonApiSerializer serializer, IExceptionHandler exceptionHandler, IETagGenerator eTagGenerator, ILoggerFactory loggerFactory)
        {
            ArgumentGuard.NotNull(serializer, nameof(serializer));
            ArgumentGuard.NotNull(exceptionHandler, nameof(exceptionHandler));
            ArgumentGuard.NotNull(eTagGenerator, nameof(eTagGenerator));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));

            _serializer = serializer;
            _exceptionHandler = exceptionHandler;
            _eTagGenerator = eTagGenerator;
            _traceWriter = new TraceLogWriter<JsonApiWriter>(loggerFactory);
        }

        /// <inheritdoc />
        public async Task WriteAsync(object model, HttpContext httpContext)
        {
            ArgumentGuard.NotNull(httpContext, nameof(httpContext));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Write response body");

            HttpRequest request = httpContext.Request;
            HttpResponse response = httpContext.Response;

            await using TextWriter writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8);
            string responseContent;

            try
            {
                responseContent = SerializeResponse(model, (HttpStatusCode)response.StatusCode);
            }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
            {
                Document document = _exceptionHandler.HandleException(exception);
                responseContent = _serializer.Serialize(document);

                response.StatusCode = (int)document.GetErrorStatusCode();
            }

            bool hasMatchingETag = SetETagResponseHeader(request, response, responseContent);

            if (hasMatchingETag)
            {
                response.StatusCode = (int)HttpStatusCode.NotModified;
                responseContent = string.Empty;
            }

            if (request.Method == HttpMethod.Head.Method)
            {
                responseContent = string.Empty;
            }

            string url = request.GetEncodedUrl();

            if (!string.IsNullOrEmpty(responseContent))
            {
                response.ContentType = _serializer.ContentType;
            }

            _traceWriter.LogMessage(() => $"Sending {response.StatusCode} response for {request.Method} request at '{url}' with body: <<{responseContent}>>");

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

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return new HttpResponseMessage(statusCode).IsSuccessStatusCode;
        }

        private static object WrapErrors(object contextObject)
        {
            if (contextObject is IEnumerable<ErrorObject> errors)
            {
                return new Document
                {
                    Errors = errors.ToList()
                };
            }

            if (contextObject is ErrorObject error)
            {
                return new Document
                {
                    Errors = error.AsList()
                };
            }

            return contextObject;
        }

        private bool SetETagResponseHeader(HttpRequest request, HttpResponse response, string responseContent)
        {
            bool isReadOnly = request.Method == HttpMethod.Get.Method || request.Method == HttpMethod.Head.Method;

            if (isReadOnly && response.StatusCode == (int)HttpStatusCode.OK)
            {
                string url = request.GetEncodedUrl();
                EntityTagHeaderValue responseETag = _eTagGenerator.Generate(url, responseContent);

                if (responseETag != null)
                {
                    response.Headers.Add(HeaderNames.ETag, responseETag.ToString());

                    return RequestContainsMatchingETag(request.Headers, responseETag);
                }
            }

            return false;
        }

        private static bool RequestContainsMatchingETag(IHeaderDictionary requestHeaders, EntityTagHeaderValue responseETag)
        {
            if (requestHeaders.Keys.Contains(HeaderNames.IfNoneMatch) &&
                EntityTagHeaderValue.TryParseList(requestHeaders[HeaderNames.IfNoneMatch], out IList<EntityTagHeaderValue> requestETags))
            {
                foreach (EntityTagHeaderValue requestETag in requestETags)
                {
                    if (responseETag.Equals(requestETag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
