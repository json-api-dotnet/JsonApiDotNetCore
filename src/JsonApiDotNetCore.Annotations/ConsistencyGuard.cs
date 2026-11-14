using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JsonApiDotNetCore;

internal static class ConsistencyGuard
{
    [ExcludeFromCodeCoverage]
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string? message = null)
    {
        if (condition)
        {
            throw new UnreachableException(message);
        }
    }
}
