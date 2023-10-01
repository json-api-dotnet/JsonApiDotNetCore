using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Queries;

internal static class SystemExpressionExtensions
{
    public static Expression CreateTupleAccessExpressionForConstant(this object? value, Type type)
    {
        // To enable efficient query plan caching, inline constants (that vary per request) should be converted into query parameters.
        // https://stackoverflow.com/questions/54075758/building-a-parameterized-entityframework-core-expression

        // This method can be used to change a query like:
        //   SELECT ... FROM ... WHERE x."Age" = 3
        // into:
        //   SELECT ... FROM ... WHERE x."Age" = @p0

        // The code below builds the next expression for a type T that is unknown at compile time:
        //   Expression.Property(Expression.Constant(Tuple.Create<T>(value)), "Item1")
        // Which represents the next C# code:
        //   Tuple.Create<T>(value).Item1;

        MethodInfo tupleCreateUnboundMethod = typeof(Tuple).GetMethods()
            .Single(method => method is { Name: "Create", IsGenericMethod: true } && method.GetGenericArguments().Length == 1);

        MethodInfo tupleCreateClosedMethod = tupleCreateUnboundMethod.MakeGenericMethod(type);

        ConstantExpression constantExpression = Expression.Constant(value, type);

        MethodCallExpression tupleCreateCall = Expression.Call(tupleCreateClosedMethod, constantExpression);
        return Expression.Property(tupleCreateCall, "Item1");
    }
}
