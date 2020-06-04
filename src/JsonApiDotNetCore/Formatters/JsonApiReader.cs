using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization.Server;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Formatters
{
    /// <inheritdoc />
    public class JsonApiReader : IJsonApiReader
    {
        private readonly IJsonApiDeserializer _deserializer;
        private readonly ICurrentRequest _currentRequest;
        private readonly ILogger<JsonApiReader> _logger;

        public JsonApiReader(IJsonApiDeserializer deserializer,
            ICurrentRequest currentRequest,
            ILoggerFactory loggerFactory)
        {
            _deserializer = deserializer;
            _currentRequest = currentRequest;
            _logger = loggerFactory.CreateLogger<JsonApiReader>();
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
            {
                return await InputFormatterResult.SuccessAsync(null);
            }

            string body = await GetRequestBody(context.HttpContext.Request.Body);

            string url = context.HttpContext.Request.GetEncodedUrl();
            _logger.LogTrace($"Received request at '{url}' with body: <<{body}>>");

            object model;
            try
            {
                _deserializer.ModelState = context.ModelState;
                model = _deserializer.Deserialize(body);
            }
            catch (InvalidRequestBodyException exception)
            {
                exception.SetRequestBody(body);
                throw;
            }
            catch (Exception exception)
            {
                throw new InvalidRequestBodyException(null, null, body, exception);
            }

            if (context.HttpContext.Request.Method == "PATCH")
            {
                bool hasMissingId = model is IList list ? HasMissingId(list) : HasMissingId(model);
                if (hasMissingId)
                {
                    throw new InvalidRequestBodyException("Payload must include id attribute.", null, body);
                }

                if (!_currentRequest.IsRelationshipPath && TryGetId(model, out var bodyId) && bodyId != _currentRequest.BaseId)
                {
                    throw new ResourceIdMismatchException(bodyId, _currentRequest.BaseId, context.HttpContext.Request.GetDisplayUrl());
                }
            }

            return await InputFormatterResult.SuccessAsync(model);
        }

        /// <summary> Checks if the deserialized payload has an ID included </summary>
        private bool HasMissingId(object model)
        {
            return TryGetId(model, out string id) && string.IsNullOrEmpty(id);
        }

        /// <summary> Checks if all elements in the deserialized payload have an ID included </summary>
        private bool HasMissingId(IEnumerable models)
        {
            foreach (var model in models)
            {
                if (TryGetId(model, out string id) && string.IsNullOrEmpty(id))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetId(object model, out string id)
        {
            if (model is ResourceObject resourceObject)
            {
                id = resourceObject.Id;
                return true;
            }

            if (model is IIdentifiable identifiable)
            {
                id = identifiable.StringId;
                return true;
            }

            id = null;
            return false;
        }

        /// <summary>
        /// Fetches the request from body asynchronously.
        /// </summary>
        /// <param name="body">Input stream for body</param>
        /// <returns>String content of body sent to server.</returns>
        private async Task<string> GetRequestBody(Stream body)
        {
            using var reader = new StreamReader(body);
            // This needs to be set to async because
            // Synchronous IO operations are 
            // https://github.com/aspnet/AspNetCore/issues/7644
            return await reader.ReadToEndAsync();
        }
    }
}
