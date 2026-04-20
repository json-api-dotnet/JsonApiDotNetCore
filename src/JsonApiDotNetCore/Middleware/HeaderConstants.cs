using JetBrains.Annotations;

namespace JsonApiDotNetCore.Middleware;

[PublicAPI]
public static class HeaderConstants
{
    [Obsolete($"Use {nameof(JsonApiMediaType)}.{nameof(JsonApiMediaType.Default)}.ToString() instead.")]
    public const string MediaType = "application/vnd.api+json";

    [Obsolete($"Use {nameof(JsonApiMediaType)}.{nameof(JsonApiMediaType.AtomicOperations)}.ToString() instead.")]
    public const string AtomicOperationsMediaType = $"{MediaType}; ext=\"https://jsonapi.org/ext/atomic\"";

    [Obsolete($"This media type is no longer needed and will be removed in a future version. " +
        $"Use {nameof(JsonApiMediaType)}.{nameof(JsonApiMediaType.AtomicOperations)}.ToString() instead.")]
    public const string RelaxedAtomicOperationsMediaType = $"{MediaType}; ext=atomic";
}
