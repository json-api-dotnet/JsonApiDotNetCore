using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

/// <summary>
/// Transforms <see cref="SortExpression" /> into
/// <see cref="Queryable.OrderBy{TSource, TKey}(IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource,TKey}})" /> calls.
/// </summary>
[PublicAPI]
public class OrderClauseBuilder : QueryClauseBuilder<Expression?>
{
    private readonly Expression _source;
    private readonly Type _extensionType;

    public OrderClauseBuilder(Expression source, LambdaScope lambdaScope, Type extensionType)
        : base(lambdaScope)
    {
        ArgumentGuard.NotNull(source);
        ArgumentGuard.NotNull(extensionType);

        _source = source;
        _extensionType = extensionType;
    }

    public Expression ApplyOrderBy(SortExpression expression)
    {
        ArgumentGuard.NotNull(expression);

        return Visit(expression, null);
    }

    public override Expression VisitSort(SortExpression expression, Expression? argument)
    {
        Expression? sortExpression = null;

        foreach (SortElementExpression sortElement in expression.Elements)
        {
            sortExpression = Visit(sortElement, sortExpression);
        }

        return sortExpression!;
    }

    public override Expression VisitSortElement(SortElementExpression expression, Expression? previousExpression)
    {
        Expression body = expression.Count != null ? Visit(expression.Count, null) : Visit(expression.TargetAttribute!, null);

        LambdaExpression lambda = Expression.Lambda(body, LambdaScope.Parameter);

        string operationName = GetOperationName(previousExpression != null, expression.IsAscending);

        return ExtensionMethodCall(previousExpression ?? _source, operationName, body.Type, lambda);
    }

    private static string GetOperationName(bool hasPrecedingSort, bool isAscending)
    {
        if (hasPrecedingSort)
        {
            return isAscending ? "ThenBy" : "ThenByDescending";
        }

        return isAscending ? "OrderBy" : "OrderByDescending";
    }

    private Expression ExtensionMethodCall(Expression source, string operationName, Type keyType, LambdaExpression keySelector)
    {
        Type[] typeArguments = ArrayFactory.Create(LambdaScope.Parameter.Type, keyType);
        return Expression.Call(_extensionType, operationName, typeArguments, source, keySelector);
    }
}
