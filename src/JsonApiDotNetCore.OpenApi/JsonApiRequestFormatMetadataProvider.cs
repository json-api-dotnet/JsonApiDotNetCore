using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.OpenApi;

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
        throw new UnreachableCodeException();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
    {
        ArgumentGuard.NotNullNorEmpty(contentType);
        ArgumentGuard.NotNull(objectType);

        if (contentType == HeaderConstants.MediaType && JsonApiSchemaFacts.IsRequestSchemaType(objectType))
        {
            return new MediaTypeCollection
            {
                new MediaTypeHeaderValue(HeaderConstants.MediaType)
            };
        }

        return new MediaTypeCollection();
    }
}
