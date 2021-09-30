using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.RequestAdapters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Serialization
{
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
        public async Task<object> ReadAsync(HttpRequest httpRequest)
        {
            ArgumentGuard.NotNull(httpRequest, nameof(httpRequest));

            string requestBody = await GetRequestBodyAsync(httpRequest);
            return GetModel(requestBody);
        }

        private async Task<string> GetRequestBodyAsync(HttpRequest httpRequest)
        {
            using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
            string requestBody = await reader.ReadToEndAsync();

            _traceWriter.LogMessage(() => $"Received {httpRequest.Method} request at '{httpRequest.GetEncodedUrl()}' with body: <<{requestBody}>>");

            return requestBody;
        }

        private object GetModel(string requestBody)
        {
            AssertHasRequestBody(requestBody);

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Read request body");

            Document document = DeserializeDocument(requestBody, _options.SerializerReadOptions);
            return ConvertDocumentToModel(document, requestBody);
        }

        [AssertionMethod]
        private static void AssertHasRequestBody(string requestBody)
        {
            if (string.IsNullOrEmpty(requestBody))
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                {
                    Title = "Missing request body."
                });
            }
        }

        private Document DeserializeDocument(string requestBody, JsonSerializerOptions serializerOptions)
        {
            try
            {
                using IDisposable _ =
                    CodeTimingSessionManager.Current.Measure("JsonSerializer.Deserialize", MeasurementSettings.ExcludeJsonSerializationInPercentages);

                return JsonSerializer.Deserialize<Document>(requestBody, serializerOptions);
            }
            catch (JsonException exception)
            {
                // JsonException.Path looks great for setting error.source.pointer, but unfortunately it is wrong in most cases.
                // This is due to the use of custom converters, which are unable to interact with internal position tracking.
                // https://github.com/dotnet/runtime/issues/50205#issuecomment-808401245
                throw new InvalidRequestBodyException(requestBody, null, exception.Message, null, null, exception);
            }
        }

        private object ConvertDocumentToModel(Document document, string requestBody)
        {
            try
            {
                return _documentAdapter.Convert(document);
            }
            catch (ModelConversionException exception)
            {
                throw new InvalidRequestBodyException(requestBody, exception.GenericMessage, exception.SpecificMessage, exception.SourcePointer,
                    exception.StatusCode, exception);
            }
        }
    }
}
