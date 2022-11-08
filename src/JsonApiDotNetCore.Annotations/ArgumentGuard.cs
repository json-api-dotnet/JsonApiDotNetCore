using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks

namespace JsonApiDotNetCore;

internal static class ArgumentGuard
{
    [AssertionMethod]
    public static void NotNull<T>([NoEnumeration] [SysNotNull] T? value, [CallerArgumentExpression("value")] string? parameterName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
    }

    [AssertionMethod]
    public static void NotNullNorEmpty<T>([SysNotNull] IEnumerable<T>? value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (!value.Any())
        {
            throw new ArgumentException("Collection cannot be null or empty.", parameterName);
        }
    }

    [AssertionMethod]
    public static void NotNullNorEmpty([SysNotNull] string? value, [CallerArgumentExpression("value")] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value == string.Empty)
        {
            throw new ArgumentException("String cannot be null or empty.", parameterName);
        }
    }
}
