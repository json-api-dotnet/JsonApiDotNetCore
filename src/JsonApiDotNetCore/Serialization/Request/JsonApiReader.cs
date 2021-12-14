using System.Net;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace JsonApiDotNetCore.Serialization.Request;

/// <inheritdoc />
public sealed class JsonApiReader : IJsonApiReader
{
    private readonly IJsonApiOptions _options;
    private readonly IDocumentAdapter _documentAdapter;
    private readonly TraceLogWriter<JsonApiReader> _traceWriter;

    public JsonApiReader(IJsonApiOptions options, IDocumentAdapter documentAdapter, ILoggerFactory loggerFactory)
    {
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(documentAdapter, nameof(documentAdapter));
        ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));

        _options = options;
        _documentAdapter = documentAdapter;
        _traceWriter = new TraceLogWriter<JsonApiReader>(loggerFactory);
    }

    /// <inheritdoc />
    public async Task<object?> ReadAsync(HttpRequest httpRequest)
    {
        ArgumentGuard.NotNull(httpRequest, nameof(httpRequest));

        string requestBody = await ReceiveRequestBodyAsync(httpRequest);

        _traceWriter.LogMessage(() => $"Received {httpRequest.Method} request at '{httpRequest.GetEncodedUrl()}' with body: <<{requestBody}>>");

        return GetModel(requestBody);
    }

    private static async Task<string> ReceiveRequestBodyAsync(HttpRequest httpRequest)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Receive request body");

        using var reader = new HttpRequestStreamReader(httpRequest.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private object? GetModel(string requestBody)
    {
        AssertHasRequestBody(requestBody);

        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Read request body");

        Document document = DeserializeDocument(requestBody);
        return ConvertDocumentToModel(document, requestBody);
    }

    [AssertionMethod]
    private static void AssertHasRequestBody(string requestBody)
    {
        if (string.IsNullOrEmpty(requestBody))
        {
            throw new InvalidRequestBodyException(null, "Missing request body.", null, null, HttpStatusCode.BadRequest);
        }
    }

    private Document DeserializeDocument(string requestBody)
    {
        try
        {
            using IDisposable _ =
                CodeTimingSessionManager.Current.Measure("JsonSerializer.Deserialize", MeasurementSettings.ExcludeJsonSerializationInPercentages);

            var document = JsonSerializer.Deserialize<Document>(requestBody, _options.SerializerReadOptions);

            AssertHasDocument(document, requestBody);

            return document;
        }
        catch (JsonException exception)
        {
            // JsonException.Path looks great for setting error.source.pointer, but unfortunately it is wrong in most cases.
            // This is due to the use of custom converters, which are unable to interact with internal position tracking.
            // https://github.com/dotnet/runtime/issues/50205#issuecomment-808401245
            throw new InvalidRequestBodyException(_options.IncludeRequestBodyInErrors ? requestBody : null, null, exception.Message, null, null, exception);
        }
    }

    private void AssertHasDocument([SysNotNull] Document? document, string requestBody)
    {
        if (document == null)
        {
            throw new InvalidRequestBodyException(_options.IncludeRequestBodyInErrors ? requestBody : null, "Expected an object, instead of 'null'.", null,
                null);
        }
    }

    private object? ConvertDocumentToModel(Document document, string requestBody)
    {
        try
        {
            return _documentAdapter.Convert(document);
        }
        catch (ModelConversionException exception)
        {
            throw new InvalidRequestBodyException(_options.IncludeRequestBodyInErrors ? requestBody : null, exception.GenericMessage, exception.SpecificMessage,
                exception.SourcePointer, exception.StatusCode, exception);
        }
    }
}
