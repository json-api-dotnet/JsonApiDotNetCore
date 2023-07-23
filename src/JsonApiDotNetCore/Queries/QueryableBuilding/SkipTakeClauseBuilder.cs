using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <inheritdoc cref="ISkipTakeClauseBuilder" />
[PublicAPI]
public class SkipTakeClauseBuilder : QueryClauseBuilder, ISkipTakeClauseBuilder
{
    public virtual Expression ApplySkipTake(PaginationExpression expression, QueryClauseBuilderContext context)
    {
        ArgumentGuard.NotNull(expression);

        return Visit(expression, context);
    }

    public override Expression VisitPagination(PaginationExpression expression, QueryClauseBuilderContext context)
    {
        Expression skipTakeExpression = context.Source;

        if (expression.PageSize != null)
        {
            int skipValue = (expression.PageNumber.OneBasedValue - 1) * expression.PageSize.Value;

            if (skipValue > 0)
            {
                skipTakeExpression = ExtensionMethodCall(skipTakeExpression, "Skip", skipValue, context);
            }

            skipTakeExpression = ExtensionMethodCall(skipTakeExpression, "Take", expression.PageSize.Value, context);
        }

        return skipTakeExpression;
    }

    private static Expression ExtensionMethodCall(Expression source, string operationName, int value, QueryClauseBuilderContext context)
    {
        Expression constant = value.CreateTupleAccessExpressionForConstant(typeof(int));

        return Expression.Call(context.ExtensionType, operationName, context.LambdaScope.Parameter.Type.AsArray(), source, constant);
    }
}
