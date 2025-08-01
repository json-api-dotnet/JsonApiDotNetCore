using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Determines the Content-Type used in OpenAPI documents for request bodies of JSON:API endpoints.
/// </summary>
internal sealed class JsonApiRequestFormatMetadataProvider : IInputFormatter, IApiRequestFormatMetadataProvider
{
    /// <inheritdoc />
    public bool CanRead(InputFormatterContext context)
    {
        return false;
    }

    /// <inheritdoc />
    public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        throw new UnreachableException();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetSupportedContentTypes(string? contentType, Type objectType)
    {
        ArgumentNullException.ThrowIfNull(objectType);

        return OpenApiContentTypeProvider.Instance.GetRequestContentTypes(objectType);
    }
}
