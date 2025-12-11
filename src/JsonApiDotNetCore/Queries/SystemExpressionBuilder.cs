using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Queries;

internal static class SystemExpressionBuilder
{
    private static readonly MethodInfo CloseOverOpenMethod =
        typeof(SystemExpressionBuilder).GetMethods().Single(method => method is { Name: nameof(CloseOver), IsGenericMethod: true });

    // To enable efficient query plan caching, inline constants (that vary per request) should be converted into query parameters.
    // https://stackoverflow.com/questions/54075758/building-a-parameterized-entityframework-core-expression
    //
    // CloseOver can be used to change a query like:
    //   SELECT ... FROM ... WHERE x."Age" = 3
    // into:
    //   SELECT ... FROM ... WHERE x."Age" = @p0

    public static Expression CloseOver(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        MethodInfo closeOverClosedMethod = CloseOverOpenMethod.MakeGenericMethod(value.GetType());
        return (Expression)closeOverClosedMethod.Invoke(null, [value])!;
    }

    public static Expression CloseOver<T>(T value)
    {
        // From https://github.com/dotnet/efcore/issues/28151#issuecomment-1374480257.
        return ((Expression<Func<T>>)(() => value)).Body;
    }
}
