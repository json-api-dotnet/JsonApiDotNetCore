using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <inheritdoc cref="IOrderClauseBuilder" />
[PublicAPI]
public class OrderClauseBuilder : QueryClauseBuilder, IOrderClauseBuilder
{
    public virtual Expression ApplyOrderBy(SortExpression expression, QueryClauseBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return Visit(expression, context);
    }

    public override Expression VisitSort(SortExpression expression, QueryClauseBuilderContext context)
    {
        QueryClauseBuilderContext nextContext = context;

        foreach (SortElementExpression sortElement in expression.Elements)
        {
            Expression sortExpression = Visit(sortElement, nextContext);
            nextContext = nextContext.WithSource(sortExpression);
        }

        return nextContext.Source;
    }

    public override Expression VisitSortElement(SortElementExpression expression, QueryClauseBuilderContext context)
    {
        Expression body = Visit(expression.Target, context);
        LambdaExpression lambda = Expression.Lambda(body, context.LambdaScope.Parameter);
        string operationName = GetOperationName(expression.IsAscending, context);

        return ExtensionMethodCall(context.Source, operationName, body.Type, lambda, context);
    }

    private static string GetOperationName(bool isAscending, QueryClauseBuilderContext context)
    {
        bool hasPrecedingSort = false;

        if (context.Source is MethodCallExpression methodCall)
        {
            hasPrecedingSort = methodCall.Method.Name is "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending";
        }

        if (hasPrecedingSort)
        {
            return isAscending ? "ThenBy" : "ThenByDescending";
        }

        return isAscending ? "OrderBy" : "OrderByDescending";
    }

    private static MethodCallExpression ExtensionMethodCall(Expression source, string operationName, Type keyType, LambdaExpression keySelector,
        QueryClauseBuilderContext context)
    {
        Type[] typeArguments =
        [
            context.LambdaScope.Parameter.Type,
            keyType
        ];

        return Expression.Call(context.ExtensionType, operationName, typeArguments, source, keySelector);
    }
}
