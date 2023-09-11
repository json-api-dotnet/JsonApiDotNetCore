using System.Net;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace JsonApiDotNetCore.Middleware;

// IMPORTANT: In your Program.cs, make sure app.UseDeveloperExceptionPage() is called BEFORE this!

public sealed class IdempotencyMiddleware
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();

    private readonly IJsonApiOptions _options;
    private readonly IFingerprintGenerator _fingerprintGenerator;
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(IJsonApiOptions options, IFingerprintGenerator fingerprintGenerator, RequestDelegate next)
    {
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(fingerprintGenerator, nameof(fingerprintGenerator));

        _options = options;
        _fingerprintGenerator = fingerprintGenerator;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IIdempotencyProvider idempotencyProvider)
    {
        try
        {
            await InnerInvokeAsync(httpContext, idempotencyProvider);
        }
        catch (JsonApiException exception)
        {
            await FlushResponseAsync(httpContext.Response, _options.SerializerWriteOptions, exception.Errors.Single());
        }
    }

    public async Task InnerInvokeAsync(HttpContext httpContext, IIdempotencyProvider idempotencyProvider)
    {
        string? idempotencyKey = GetIdempotencyKey(httpContext.Request.Headers);

        if (idempotencyKey != null && idempotencyProvider is NoIdempotencyProvider)
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = $"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.",
                Detail = "Idempotency is currently disabled.",
                Source = new ErrorSource
                {
                    Header = HeaderConstants.IdempotencyKey
                }
            });
        }

        if (!idempotencyProvider.IsSupported(httpContext.Request))
        {
            await _next(httpContext);
            return;
        }

        AssertIdempotencyKeyIsValid(idempotencyKey);

        await BufferRequestBodyAsync(httpContext);

        string requestFingerprint = await GetRequestFingerprintAsync(httpContext);
        IdempotentResponse? idempotentResponse = await idempotencyProvider.GetResponseFromCacheAsync(idempotencyKey, httpContext.RequestAborted);

        if (idempotentResponse != null)
        {
            if (idempotentResponse.RequestFingerprint != requestFingerprint)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.UnprocessableEntity)
                {
                    Title = $"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.",
                    Detail = $"The provided idempotency key '{idempotencyKey}' is in use for another request.",
                    Source = new ErrorSource
                    {
                        Header = HeaderConstants.IdempotencyKey
                    }
                });
            }

            httpContext.Response.StatusCode = (int)idempotentResponse.ResponseStatusCode;
            httpContext.Response.Headers[HeaderConstants.IdempotencyKey] = $"\"{idempotencyKey}\"";
            httpContext.Response.Headers[HeaderNames.Location] = idempotentResponse.ResponseLocationHeader;

            if (idempotentResponse.ResponseContentTypeHeader != null)
            {
                // Workaround for invalid nullability annotation in HttpResponse.ContentType
                // Fixed after ASP.NET 6 release, see https://github.com/dotnet/aspnetcore/commit/8bb128185b58a26065d0f29e695a2410cf0a3c68#diff-bbfd771a8ef013a9921bff36df0d69f424910e079945992f1dccb24de54ca717
                httpContext.Response.ContentType = idempotentResponse.ResponseContentTypeHeader;
            }

            await using TextWriter writer = new HttpResponseStreamWriter(httpContext.Response.Body, Encoding.UTF8);
            await writer.WriteAsync(idempotentResponse.ResponseBody);
            await writer.FlushAsync();

            return;
        }

        await using IOperationsTransaction transaction =
            await idempotencyProvider.BeginRequestAsync(idempotencyKey, requestFingerprint, httpContext.RequestAborted);

        string responseBody = await CaptureResponseBodyAsync(httpContext, _next);

        idempotentResponse = new IdempotentResponse(requestFingerprint, (HttpStatusCode)httpContext.Response.StatusCode,
            httpContext.Response.Headers[HeaderNames.Location], httpContext.Response.ContentType, responseBody);

        await idempotencyProvider.CompleteRequestAsync(idempotencyKey, idempotentResponse, transaction, httpContext.RequestAborted);
    }

    private static string? GetIdempotencyKey(IHeaderDictionary requestHeaders)
    {
        if (!requestHeaders.ContainsKey(HeaderConstants.IdempotencyKey))
        {
            return null;
        }

        string headerValue = requestHeaders[HeaderConstants.IdempotencyKey];

        if (headerValue.Length >= 2 && headerValue[0] == '\"' && headerValue[^1] == '\"')
        {
            return headerValue[1..^1];
        }

        return string.Empty;
    }

    [AssertionMethod]
    private static void AssertIdempotencyKeyIsValid([SysNotNull] string? idempotencyKey)
    {
        if (idempotencyKey == null)
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = $"Missing '{HeaderConstants.IdempotencyKey}' HTTP header.",
                Detail = "An idempotency key is a unique value generated by the client, which the server uses to recognize subsequent retries " +
                    "of the same request. This should be a random string with enough entropy to avoid collisions."
            });
        }

        if (idempotencyKey == string.Empty)
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = $"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.",
                Detail = "Expected non-empty value surrounded by double quotes.",
                Source = new ErrorSource
                {
                    Header = HeaderConstants.IdempotencyKey
                }
            });
        }
    }

    /// <summary>
    /// Enables to read the HTTP request stream multiple times, without risking GC Gen2/LOH promotion.
    /// </summary>
    private static async Task BufferRequestBodyAsync(HttpContext httpContext)
    {
        // Above this threshold, EnableBuffering() switches to a temporary file on disk.
        // Source: Microsoft.AspNetCore.Http.BufferingHelper.DefaultBufferThreshold
        const int enableBufferingThreshold = 1024 * 30;

        if (httpContext.Request.ContentLength > enableBufferingThreshold)
        {
            httpContext.Request.EnableBuffering(enableBufferingThreshold);
        }
        else
        {
            MemoryStream memoryRequestBodyStream = MemoryStreamManager.GetStream();
            await httpContext.Request.Body.CopyToAsync(memoryRequestBodyStream, httpContext.RequestAborted);
            memoryRequestBodyStream.Seek(0, SeekOrigin.Begin);

            httpContext.Request.Body = memoryRequestBodyStream;
            httpContext.Response.RegisterForDispose(memoryRequestBodyStream);
        }
    }

    private async Task<string> GetRequestFingerprintAsync(HttpContext httpContext)
    {
        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        string requestBody = await reader.ReadToEndAsync();
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

        return _fingerprintGenerator.Generate(ArrayFactory.Create(httpContext.Request.GetEncodedUrl(), requestBody));
    }

    /// <summary>
    /// Executes the specified action and returns what it wrote to the HTTP response stream.
    /// </summary>
    private static async Task<string> CaptureResponseBodyAsync(HttpContext httpContext, RequestDelegate nextAction)
    {
        // Loosely based on https://elanderson.net/2019/12/log-requests-and-responses-in-asp-net-core-3/.

        Stream previousResponseBodyStream = httpContext.Response.Body;

        try
        {
            await using MemoryStream memoryResponseBodyStream = MemoryStreamManager.GetStream();
            httpContext.Response.Body = memoryResponseBodyStream;

            try
            {
                await nextAction(httpContext);
            }
            finally
            {
                memoryResponseBodyStream.Seek(0, SeekOrigin.Begin);
                await memoryResponseBodyStream.CopyToAsync(previousResponseBodyStream);
            }

            memoryResponseBodyStream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(memoryResponseBodyStream, leaveOpen: true);
            return await streamReader.ReadToEndAsync();
        }
        finally
        {
            httpContext.Response.Body = previousResponseBodyStream;
        }
    }

    private static async Task FlushResponseAsync(HttpResponse httpResponse, JsonSerializerOptions serializerOptions, ErrorObject error)
    {
        httpResponse.ContentType = HeaderConstants.MediaType;
        httpResponse.StatusCode = (int)error.StatusCode;

        var errorDocument = new Document
        {
            Errors = error.AsList()
        };

        await JsonSerializer.SerializeAsync(httpResponse.Body, errorDocument, serializerOptions);
        await httpResponse.Body.FlushAsync();
    }
}
