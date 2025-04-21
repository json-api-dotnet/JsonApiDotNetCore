using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

#pragma warning disable AV1008 // Class should not be static

internal static class OpenApiMediaTypeExtension
{
    public const string ExtensionNamespace = "openapi";
    public const string DiscriminatorPropertyName = "discriminator";
    public const string FullyQualifiedOpenApiDiscriminatorPropertyName = $"{ExtensionNamespace}:{DiscriminatorPropertyName}";
    public static readonly JsonApiMediaTypeExtension OpenApi = new("https://www.jsonapi.net/ext/openapi");
    public static readonly JsonApiMediaTypeExtension RelaxedOpenApi = new("openapi");
}
