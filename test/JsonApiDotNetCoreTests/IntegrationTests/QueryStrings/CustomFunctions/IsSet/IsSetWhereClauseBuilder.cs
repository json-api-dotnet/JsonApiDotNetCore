using System.Linq.Expressions;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsSet;

internal sealed class IsSetWhereClauseBuilder : WhereClauseBuilder
{
    public override Expression DefaultVisit(QueryExpression expression, QueryClauseBuilderContext context)
    {
        if (expression is IsSetExpression isSetExpression)
        {
            return VisitIsSet(isSetExpression, context);
        }

        return base.DefaultVisit(expression, context);
    }

    private BinaryExpression VisitIsSet(IsSetExpression expression, QueryClauseBuilderContext context)
    {
        Expression propertyAccess = Visit(expression.Target, context);
        return Expression.NotEqual(propertyAccess, Expression.Constant(null, propertyAccess.Type));
    }
}
