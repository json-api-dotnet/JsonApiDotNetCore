using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable format

namespace JsonApiDotNetCore.OpenApi.Client.NSwag;

internal static class ArgumentGuard
{
    [AssertionMethod]
    public static void NotNull<T>([NoEnumeration] [SysNotNull] T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
    }
}
