using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks

namespace JsonApiDotNetCore.OpenApi.Client;

internal static class ArgumentGuard
{
    [AssertionMethod]
    public static void NotNull<T>([NoEnumeration] [SysNotNull] T? value, [CallerArgumentExpression("value")] string? parameterName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
    }
}
