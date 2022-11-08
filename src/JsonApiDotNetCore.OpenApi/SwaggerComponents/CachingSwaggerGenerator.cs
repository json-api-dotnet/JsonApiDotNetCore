using System.Collections.Concurrent;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

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
        ArgumentGuard.NotNull(defaultSwaggerGenerator);
        _defaultSwaggerGenerator = defaultSwaggerGenerator;
    }

    public OpenApiDocument GetSwagger(string documentName, string? host = null, string? basePath = null)
    {
        ArgumentGuard.NotNullNorEmpty(documentName);

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
