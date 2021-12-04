using System.Net;
using System.Text;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
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

namespace JsonApiDotNetCore.Serialization.Response;

/// <inheritdoc />
public sealed class JsonApiWriter : IJsonApiWriter
{
    private readonly IJsonApiRequest _request;
    private readonly IJsonApiOptions _options;
    private readonly IResponseModelAdapter _responseModelAdapter;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IETagGenerator _eTagGenerator;
    private readonly TraceLogWriter<JsonApiWriter> _traceWriter;

    public JsonApiWriter(IJsonApiRequest request, IJsonApiOptions options, IResponseModelAdapter responseModelAdapter, IExceptionHandler exceptionHandler,
        IETagGenerator eTagGenerator, ILoggerFactory loggerFactory)
    {
        ArgumentGuard.NotNull(request, nameof(request));
        ArgumentGuard.NotNull(responseModelAdapter, nameof(responseModelAdapter));
        ArgumentGuard.NotNull(exceptionHandler, nameof(exceptionHandler));
        ArgumentGuard.NotNull(eTagGenerator, nameof(eTagGenerator));
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));

        _request = request;
        _options = options;
        _responseModelAdapter = responseModelAdapter;
        _exceptionHandler = exceptionHandler;
        _eTagGenerator = eTagGenerator;
        _traceWriter = new TraceLogWriter<JsonApiWriter>(loggerFactory);
    }

    /// <inheritdoc />
    public async Task WriteAsync(object? model, HttpContext httpContext)
    {
        ArgumentGuard.NotNull(httpContext, nameof(httpContext));

        if (model == null && !CanWriteBody((HttpStatusCode)httpContext.Response.StatusCode))
        {
            // Prevent exception from Kestrel server, caused by writing data:null json response.
            return;
        }

        string? responseBody = GetResponseBody(model, httpContext);

        if (httpContext.Request.Method == HttpMethod.Head.Method)
        {
            httpContext.Response.GetTypedHeaders().ContentLength = responseBody == null ? 0 : Encoding.UTF8.GetByteCount(responseBody);
            return;
        }

        _traceWriter.LogMessage(() =>
            $"Sending {httpContext.Response.StatusCode} response for {httpContext.Request.Method} request at '{httpContext.Request.GetEncodedUrl()}' with body: <<{responseBody}>>");

        await SendResponseBodyAsync(httpContext.Response, responseBody);
    }

    private static bool CanWriteBody(HttpStatusCode statusCode)
    {
        return statusCode is not HttpStatusCode.NoContent and not HttpStatusCode.ResetContent and not HttpStatusCode.NotModified;
    }

    private string? GetResponseBody(object? model, HttpContext httpContext)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Write response body");

        try
        {
            if (model is ProblemDetails problemDetails)
            {
                throw new UnsuccessfulActionResultException(problemDetails);
            }

            if (model == null && !IsSuccessStatusCode((HttpStatusCode)httpContext.Response.StatusCode))
            {
                throw new UnsuccessfulActionResultException((HttpStatusCode)httpContext.Response.StatusCode);
            }

            string responseBody = RenderModel(model);

            if (SetETagResponseHeader(httpContext.Request, httpContext.Response, responseBody))
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                return null;
            }

            return responseBody;
        }
#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
        catch (Exception exception)
#pragma warning restore AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException
        {
            IReadOnlyList<ErrorObject> errors = _exceptionHandler.HandleException(exception);
            httpContext.Response.StatusCode = (int)ErrorObject.GetResponseStatusCode(errors);

            return RenderModel(errors);
        }
    }

    private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage(statusCode).IsSuccessStatusCode;
    }

    private string RenderModel(object? model)
    {
        Document document = _responseModelAdapter.Convert(model);
        return SerializeDocument(document);
    }

    private string SerializeDocument(Document document)
    {
        using IDisposable _ =
            CodeTimingSessionManager.Current.Measure("JsonSerializer.Serialize", MeasurementSettings.ExcludeJsonSerializationInPercentages);

        return JsonSerializer.Serialize(document, _options.SerializerWriteOptions);
    }

    private bool SetETagResponseHeader(HttpRequest request, HttpResponse response, string responseContent)
    {
        bool isReadOnly = request.Method == HttpMethod.Get.Method || request.Method == HttpMethod.Head.Method;

        if (isReadOnly && response.StatusCode == (int)HttpStatusCode.OK)
        {
            string url = request.GetEncodedUrl();
            EntityTagHeaderValue responseETag = _eTagGenerator.Generate(url, responseContent);

            response.Headers.Add(HeaderNames.ETag, responseETag.ToString());

            return RequestContainsMatchingETag(request.Headers, responseETag);
        }

        return false;
    }

    private static bool RequestContainsMatchingETag(IHeaderDictionary requestHeaders, EntityTagHeaderValue responseETag)
    {
        if (requestHeaders.Keys.Contains(HeaderNames.IfNoneMatch) &&
            EntityTagHeaderValue.TryParseList(requestHeaders[HeaderNames.IfNoneMatch], out IList<EntityTagHeaderValue>? requestETags))
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

    private async Task SendResponseBodyAsync(HttpResponse httpResponse, string? responseBody)
    {
        if (!string.IsNullOrEmpty(responseBody))
        {
            httpResponse.ContentType =
                _request.Kind == EndpointKind.AtomicOperations ? HeaderConstants.AtomicOperationsMediaType : HeaderConstants.MediaType;

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Send response body");

            await using TextWriter writer = new HttpResponseStreamWriter(httpResponse.Body, Encoding.UTF8);
            await writer.WriteAsync(responseBody);
            await writer.FlushAsync();
        }
    }
}
