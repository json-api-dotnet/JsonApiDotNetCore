using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    public class JsonApiReader : IJsonApiReader
    {
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IJsonApiRequest _request;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly TraceLogWriter<JsonApiReader> _traceWriter;

        public JsonApiReader(IJsonApiDeserializer deserializer,
            IJsonApiRequest request,
            IResourceContextProvider resourceContextProvider,
            ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _resourceContextProvider = resourceContextProvider ??  throw new ArgumentNullException(nameof(resourceContextProvider));
            _traceWriter = new TraceLogWriter<JsonApiReader>(loggerFactory);
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
            _traceWriter.LogMessage(() => $"Received request at '{url}' with body: <<{body}>>");

            object model;
            try
            {
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

            ValidateRequestIncludesId(context, model, body);

            ValidateIncomingResourceType(context, model);
            
            return await InputFormatterResult.SuccessAsync(model);
        }

        // TODO: Consider moving these assertions to RequestDeserializer. See next todo.
        private void ValidateIncomingResourceType(InputFormatterContext context, object model)
        {
            if (context.HttpContext.IsJsonApiRequest() && context.HttpContext.Request.Method != HttpMethods.Get)
            {
                var endpointResourceType = GetEndpointResourceType();
                if (endpointResourceType == null)
                {
                    return;
                }
                
                var bodyResourceTypes = GetBodyResourceTypes(model);
                foreach (var bodyResourceType in bodyResourceTypes)
                {
                    if (!endpointResourceType.IsAssignableFrom(bodyResourceType))
                    {
                        var resourceFromEndpoint = _resourceContextProvider.GetResourceContext(endpointResourceType);
                        var resourceFromBody = _resourceContextProvider.GetResourceContext(bodyResourceType);

                        throw new ResourceTypeMismatchException(new HttpMethod(context.HttpContext.Request.Method),
                            context.HttpContext.Request.Path,
                            resourceFromEndpoint, resourceFromBody);
                    }
                }
            }
        }

        // TODO: Consider moving these assertions to RequestDeserializer.
        // Right now, BaseDeserializer is responsible for throwing errors when id/type is missing in scenarios that this is ALWAYS true,
        // regardless of server/client side deserialization. The assertions below are only relevant for server deserializers since they depend on
        // IJsonApiRequest and HttpContextAccessor. Right now these two are already used to check the http request method and endpoint kind, so might as well move this into there.
        // Additional up side: testability improves.
        private void ValidateRequestIncludesId(InputFormatterContext context, object model, string body)
        {
            if (context.HttpContext.Request.Method == HttpMethods.Patch || _request.Kind == EndpointKind.Relationship)
            {
                bool hasMissingId = model is IEnumerable collection ? HasMissingId(collection) : HasMissingId(model);
                if (hasMissingId)
                {
                    throw new InvalidRequestBodyException("Request body must include 'id' element.", "Expected 'id' element in 'data' element.", body);
                }
                
                if (_request.Kind == EndpointKind.Primary && TryGetId(model, out var bodyId) && bodyId != _request.PrimaryId)
                {
                    throw new ResourceIdMismatchException(bodyId, _request.PrimaryId, context.HttpContext.Request.GetDisplayUrl());
                }
            }
        }

        /// <summary> Checks if the deserialized request body has an ID included </summary>
        private bool HasMissingId(object model)
        {
            return TryGetId(model, out string id) && string.IsNullOrEmpty(id);
        }

        /// <summary> Checks if all elements in the deserialized request body have an ID included </summary>
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

        private IEnumerable<Type> GetBodyResourceTypes(object model)
        {
            if (model is IEnumerable<IIdentifiable> resourceCollection)
            {
                return resourceCollection.Select(r => r.GetType()).Distinct();
            }

            return model == null ? Array.Empty<Type>() : new[] { model.GetType() };
        }

        private Type GetEndpointResourceType()
        {
            return _request.Kind == EndpointKind.Primary
                ? _request.PrimaryResource.ResourceType 
                : _request.SecondaryResource?.ResourceType;
        }
    }
}
