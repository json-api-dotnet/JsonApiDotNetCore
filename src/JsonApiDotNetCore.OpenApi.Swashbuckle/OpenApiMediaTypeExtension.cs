using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiMediaTypeExtension
{
    public const string ExtensionNamespace = "openapi";
    public const string DiscriminatorPropertyName = "discriminator";
    public const string FullyQualifiedOpenApiDiscriminatorPropertyName = $"{ExtensionNamespace}:{DiscriminatorPropertyName}";
    public static readonly JsonApiMediaTypeExtension OpenApi = new("https://www.jsonapi.net/ext/openapi");

    [Obsolete("This media type is no longer needed and will be removed in a future version. Use OpenApi instead.")]
    public static readonly JsonApiMediaTypeExtension RelaxedOpenApi = new("openapi");
}
