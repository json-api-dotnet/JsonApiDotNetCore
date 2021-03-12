using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Application-wide entry point for writing JSON:API response bodies.
    /// </summary>
    public abstract class JsonApiFormatter : IApiRequestFormatMetadataProvider
    {
        private readonly Type _operationContainerType = typeof(OperationContainer);

        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            ArgumentGuard.NotNull(contentType, nameof(contentType));
            ArgumentGuard.NotNull(objectType, nameof(objectType));

            string mediaType = IsAtomicOperationsType(objectType) ? HeaderConstants.AtomicOperationsMediaType : HeaderConstants.MediaType;

            return new MediaTypeCollection
            {
                new MediaTypeHeaderValue(mediaType)
            };
        }

        private bool IsAtomicOperationsType(Type objectType)
        {
            return objectType.GetInterface(nameof(IEnumerable)) != null && objectType.GetGenericArguments().First() == _operationContainerType;
        }
    }
}
