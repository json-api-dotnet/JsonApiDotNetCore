using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace JsonApiDotNetCore;

internal static class ArgumentGuard
{
    [AssertionMethod]
    public static void NotNullNorEmpty<T>([SysNotNull] IEnumerable<T>? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (!value.Any())
        {
            throw new ArgumentException("Collection cannot be null or empty.", parameterName);
        }
    }
}
