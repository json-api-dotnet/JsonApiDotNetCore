using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

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
