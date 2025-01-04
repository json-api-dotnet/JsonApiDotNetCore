using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Middleware;

[PublicAPI]
public static class HeaderConstants
{
    [Obsolete($"Use {nameof(JsonApiMediaType)}.{nameof(JsonApiMediaType.Default)}.ToString() instead.")]
    public const string MediaType = "application/vnd.api+json";

    [Obsolete($"Use {nameof(JsonApiMediaType)}.{nameof(JsonApiMediaType.AtomicOperations)}.ToString() instead.")]
    public const string AtomicOperationsMediaType = $"{MediaType}; ext=\"https://jsonapi.org/ext/atomic\"";

    [Obsolete($"Use {nameof(JsonApiMediaType)}.{nameof(JsonApiMediaType.RelaxedAtomicOperations)}.ToString() instead.")]
    public const string RelaxedAtomicOperationsMediaType = $"{MediaType}; ext=atomic";
}
