using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

internal sealed class LengthWhereClauseBuilder : WhereClauseBuilder
{
    private static readonly MethodInfo LengthPropertyGetter = typeof(string).GetProperty("Length")!.GetGetMethod()!;

    public override Expression DefaultVisit(QueryExpression expression, QueryClauseBuilderContext context)
    {
        if (expression is LengthExpression lengthExpression)
        {
            return VisitLength(lengthExpression, context);
        }

        return base.DefaultVisit(expression, context);
    }

    private MemberExpression VisitLength(LengthExpression expression, QueryClauseBuilderContext context)
    {
        Expression propertyAccess = Visit(expression.TargetAttribute, context);
        return Expression.Property(propertyAccess, LengthPropertyGetter);
    }
}
