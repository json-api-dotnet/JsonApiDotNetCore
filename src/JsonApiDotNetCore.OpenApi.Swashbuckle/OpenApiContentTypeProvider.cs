using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Determines the Content-Type used in OpenAPI documents for request/response bodies of JSON:API endpoints.
/// </summary>
internal sealed class OpenApiContentTypeProvider
{
    public static OpenApiContentTypeProvider Instance { get; } = new();

    private OpenApiContentTypeProvider()
    {
    }

    public IReadOnlyList<string> GetRequestContentTypes(Type documentType)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        // Don't return multiple media types, see https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1729#issuecomment-2972032608.

        if (documentType == typeof(OperationsRequestDocument))
        {
            return [OpenApiMediaTypes.AtomicOperationsWithOpenApi.ToString()];
        }

        if (JsonApiSchemaFacts.IsRequestDocumentSchemaType(documentType))
        {
            return [OpenApiMediaTypes.OpenApi.ToString()];
        }

        return [];
    }

    public IReadOnlyList<string> GetResponseContentTypes(Type? documentType)
    {
        // Don't return multiple media types, see https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1729#issuecomment-2972032608.

        if (documentType == typeof(OperationsResponseDocument))
        {
            return [OpenApiMediaTypes.AtomicOperationsWithOpenApi.ToString()];
        }

        if (documentType == typeof(ErrorResponseDocument))
        {
            return [OpenApiMediaTypes.OpenApi.ToString()];
        }

        if (documentType != null && JsonApiSchemaFacts.IsResponseDocumentSchemaType(documentType))
        {
            return [OpenApiMediaTypes.OpenApi.ToString()];
        }

        return [];
    }
}
