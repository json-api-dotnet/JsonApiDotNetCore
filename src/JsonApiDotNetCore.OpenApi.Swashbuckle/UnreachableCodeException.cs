#pragma warning disable CA1064 // Exceptions should be public

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class UnreachableCodeException()
    : Exception("This code should not be reachable.");
