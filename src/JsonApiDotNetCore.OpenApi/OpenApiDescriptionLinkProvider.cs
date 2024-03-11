using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

/// <summary>
/// Provides the OpenAPI URL for the "describedby" link in https://jsonapi.org/format/#document-top-level.
/// </summary>
internal sealed class OpenApiDescriptionLinkProvider : IDocumentDescriptionLinkProvider
{
    private readonly IOptionsMonitor<SwaggerGeneratorOptions> _swaggerGeneratorOptionsMonitor;
    private readonly IOptionsMonitor<SwaggerOptions> _swaggerOptionsMonitor;

    public OpenApiDescriptionLinkProvider(IOptionsMonitor<SwaggerGeneratorOptions> swaggerGeneratorOptionsMonitor,
        IOptionsMonitor<SwaggerOptions> swaggerOptionsMonitor)
    {
        ArgumentGuard.NotNull(swaggerGeneratorOptionsMonitor);
        ArgumentGuard.NotNull(swaggerOptionsMonitor);

        _swaggerGeneratorOptionsMonitor = swaggerGeneratorOptionsMonitor;
        _swaggerOptionsMonitor = swaggerOptionsMonitor;
    }

    /// <inheritdoc />
    public string? GetUrl()
    {
        SwaggerGeneratorOptions swaggerGeneratorOptions = _swaggerGeneratorOptionsMonitor.CurrentValue;

        if (swaggerGeneratorOptions.SwaggerDocs.Count > 0)
        {
            string latestVersionDocumentName = swaggerGeneratorOptions.SwaggerDocs.Last().Key;

            SwaggerOptions swaggerOptions = _swaggerOptionsMonitor.CurrentValue;
            return swaggerOptions.RouteTemplate.Replace("{documentName}", latestVersionDocumentName).Replace("{json|yaml}", "json");
        }

        return null;
    }
}
