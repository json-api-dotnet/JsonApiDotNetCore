using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

internal sealed class IsUpperCaseWhereClauseBuilder : WhereClauseBuilder
{
    private static readonly MethodInfo ToUpperMethod = typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!;

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
        Expression propertyAccess = Visit(expression.TargetAttribute, context);
        MethodCallExpression toUpperMethodCall = Expression.Call(propertyAccess, ToUpperMethod);

        return Expression.Equal(propertyAccess, toUpperMethodCall);
    }
}
