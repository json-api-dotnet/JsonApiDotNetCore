using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

#pragma warning disable AV1008 // Class should not be static

internal static class ConsistencyGuard
{
    [ExcludeFromCodeCoverage]
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition)
    {
        if (condition)
        {
            throw new UnreachableException();
        }
    }
}
