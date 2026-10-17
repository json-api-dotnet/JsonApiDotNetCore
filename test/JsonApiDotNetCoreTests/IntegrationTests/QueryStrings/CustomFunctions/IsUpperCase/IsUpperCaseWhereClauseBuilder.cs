using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

internal sealed class IsUpperCaseWhereClauseBuilder : WhereClauseBuilder
{
    private static readonly MethodInfo ToUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;

    private static readonly MethodInfo CompareMethod = typeof(string).GetMethod("Compare", [
        typeof(string),
        typeof(string)
    ])!;

    private static readonly ConstantExpression ZeroConstantExpression = Expression.Constant(0);

    public override Expression DefaultVisit(QueryExpression expression, QueryClauseBuilderContext context)
    {
        if (expression is IsUpperCaseExpression isUpperCaseExpression)
        {
            return VisitIsUpperCase(isUpperCaseExpression, context);
        }

        return base.DefaultVisit(expression, context);
    }

    private BinaryExpression VisitIsUpperCase(IsUpperCaseExpression expression, QueryClauseBuilderContext context)
    {
        Expression propertyAccess = Visit(expression.Target, context);
        MethodCallExpression toUpperMethodCall = Expression.Call(propertyAccess, ToUpperMethod);

        if (expression.Target is ScalarCollectionElementValueExpression)
        {
            // Workaround for https://github.com/dotnet/efcore/issues/39351.
            MethodCallExpression compareCall = Expression.Call(CompareMethod, propertyAccess, toUpperMethodCall);
            return Expression.Equal(compareCall, ZeroConstantExpression);
        }

        return Expression.Equal(propertyAccess, toUpperMethodCall);
    }
}
