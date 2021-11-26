using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    /// <summary>
    /// The default <see cref="ISwaggerProvider" /> implementation re-renders the OpenApiDocument every time it is requested, which is redundant in our case.
    /// This implementation provides a very basic caching layer.
    /// </summary>
    internal sealed class CachingSwaggerGenerator : ISwaggerProvider
    {
        private readonly SwaggerGenerator _defaultSwaggerGenerator;
        private readonly ConcurrentDictionary<string, OpenApiDocument> _openApiDocumentCache = new();

        public CachingSwaggerGenerator(SwaggerGenerator defaultSwaggerGenerator)
        {
            ArgumentGuard.NotNull(defaultSwaggerGenerator, nameof(defaultSwaggerGenerator));
            _defaultSwaggerGenerator = defaultSwaggerGenerator;
        }

        public OpenApiDocument GetSwagger(string documentName, string? host = null, string? basePath = null)
        {
            ArgumentGuard.NotNullNorEmpty(documentName, nameof(documentName));

            string cacheKey = $"{documentName}#{host}#{basePath}";

            return _openApiDocumentCache.GetOrAdd(cacheKey, _ =>
            {
                OpenApiDocument document = _defaultSwaggerGenerator.GetSwagger(documentName, host, basePath);

                // Remove once https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2283 is addressed.
                document.Components.Schemas = new SortedDictionary<string, OpenApiSchema>(document.Components.Schemas, StringComparer.Ordinal);
                return document;
            });
        }
    }
}
