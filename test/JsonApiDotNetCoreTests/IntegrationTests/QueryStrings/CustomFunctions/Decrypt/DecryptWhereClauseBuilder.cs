using System.Linq.Expressions;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.QueryableBuilding;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

internal sealed class DecryptWhereClauseBuilder : WhereClauseBuilder
{
    public override Expression DefaultVisit(QueryExpression expression, QueryClauseBuilderContext context)
    {
        if (expression is DecryptExpression decryptExpression)
        {
            return VisitDecrypt(decryptExpression, context);
        }

        return base.DefaultVisit(expression, context);
    }

    private MethodCallExpression VisitDecrypt(DecryptExpression expression, QueryClauseBuilderContext context)
    {
        Expression propertyAccess = Visit(expression.TargetAttribute, context);
        return Expression.Call(null, DatabaseFunctionStub.DecryptMethod, propertyAccess);
    }
}
