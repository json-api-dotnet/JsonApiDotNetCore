using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable format

namespace JsonApiDotNetCore;

internal static class ArgumentGuard
{
    [AssertionMethod]
    public static void NotNull<T>([NoEnumeration] [SysNotNull] T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
    }

    [AssertionMethod]
    public static void NotNullNorEmpty<T>([SysNotNull] IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (!value.Any())
        {
            throw new ArgumentException("Collection cannot be null or empty.", parameterName);
        }
    }

    [AssertionMethod]
    public static void NotNullNorEmpty([SysNotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if !NET6_0
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
#else
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length == 0)
        {
            throw new ArgumentException("String cannot be null or empty.", parameterName);
        }
#endif
    }

    [AssertionMethod]
    public static void NotNullNorWhitespace([SysNotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if !NET6_0
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
#else
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("String cannot be null, empty, or whitespace.", parameterName);
        }
#endif
    }
}
