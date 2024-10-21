using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class JsonApiRequestFormatMetadataProvider : IInputFormatter, IApiRequestFormatMetadataProvider
{
    private static readonly string DefaultMediaType = JsonApiMediaType.Default.ToString();

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

        if (JsonApiSchemaFacts.IsRequestBodySchemaType(objectType) && MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue? headerValue) &&
            headerValue.MediaType.Equals(DefaultMediaType, StringComparison.OrdinalIgnoreCase))
        {
            return new MediaTypeCollection
            {
                headerValue
            };
        }

        return [];
    }
}
