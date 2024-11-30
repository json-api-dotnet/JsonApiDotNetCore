using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

#pragma warning disable AV1008 // Class should not be static

internal static class OpenApiMediaTypeExtension
{
    // The discriminator only exists to guide OpenAPI codegen of request bodies. It has no meaning in JsonApiDotNetCore.
    public const string ExtensionNamespace = "openapi";
    public const string DiscriminatorPropertyName = "discriminator";
    public const string FullyQualifiedOpenApiDiscriminatorPropertyName = $"{ExtensionNamespace}:{DiscriminatorPropertyName}";

    // TODO: Write documentation page at where this link points to.
    public static readonly JsonApiMediaTypeExtension OpenApi = new("https://www.jsonapi.net/ext/openapi");
    public static readonly JsonApiMediaTypeExtension RelaxedOpenApi = new("openapi");
}
