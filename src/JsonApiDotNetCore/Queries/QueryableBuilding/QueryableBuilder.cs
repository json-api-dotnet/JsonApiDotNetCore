using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <inheritdoc cref="IQueryableBuilder" />
[PublicAPI]
public class QueryableBuilder : IQueryableBuilder
{
    private readonly IIncludeClauseBuilder _includeClauseBuilder;
    private readonly IWhereClauseBuilder _whereClauseBuilder;
    private readonly IOrderClauseBuilder _orderClauseBuilder;
    private readonly ISkipTakeClauseBuilder _skipTakeClauseBuilder;
    private readonly ISelectClauseBuilder _selectClauseBuilder;

    public QueryableBuilder(IIncludeClauseBuilder includeClauseBuilder, IWhereClauseBuilder whereClauseBuilder, IOrderClauseBuilder orderClauseBuilder,
        ISkipTakeClauseBuilder skipTakeClauseBuilder, ISelectClauseBuilder selectClauseBuilder)
    {
        ArgumentNullException.ThrowIfNull(includeClauseBuilder);
        ArgumentNullException.ThrowIfNull(whereClauseBuilder);
        ArgumentNullException.ThrowIfNull(orderClauseBuilder);
        ArgumentNullException.ThrowIfNull(skipTakeClauseBuilder);
        ArgumentNullException.ThrowIfNull(selectClauseBuilder);

        _includeClauseBuilder = includeClauseBuilder;
        _whereClauseBuilder = whereClauseBuilder;
        _orderClauseBuilder = orderClauseBuilder;
        _skipTakeClauseBuilder = skipTakeClauseBuilder;
        _selectClauseBuilder = selectClauseBuilder;
    }

    public virtual Expression ApplyQuery(QueryLayer layer, QueryableBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(context);
        AssertSameType(layer.ResourceType, context.ElementType);

        Expression expression = context.Source;

        if (layer.Include != null)
        {
            expression = ApplyInclude(expression, layer.Include, layer.ResourceType, context);
        }

        if (layer.Filter != null)
        {
            expression = ApplyFilter(expression, layer.Filter, layer.ResourceType, context);
        }

        if (layer.Sort != null)
        {
            expression = ApplySort(expression, layer.Sort, layer.ResourceType, context);
        }

        if (layer.Pagination != null)
        {
            expression = ApplyPagination(expression, layer.Pagination, layer.ResourceType, context);
        }

        if (layer.Selection != null)
        {
            expression = ApplySelection(expression, layer.Selection, layer.ResourceType, context);
        }

        return expression;
    }

    private static void AssertSameType(ResourceType resourceType, Type elementType)
    {
        if (elementType != resourceType.ClrType)
        {
            throw new InvalidOperationException(
                $"Internal error: Mismatch between query layer type '{resourceType.ClrType.Name}' and query element type '{elementType.Name}'.");
        }
    }

    protected virtual Expression ApplyInclude(Expression source, IncludeExpression include, ResourceType resourceType, QueryableBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(include);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(context);

        using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(context.ElementType);
        QueryClauseBuilderContext clauseContext = context.CreateClauseContext(this, source, resourceType, lambdaScope);

        return _includeClauseBuilder.ApplyInclude(include, clauseContext);
    }

    protected virtual Expression ApplyFilter(Expression source, FilterExpression filter, ResourceType resourceType, QueryableBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(context);

        using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(context.ElementType);
        QueryClauseBuilderContext clauseContext = context.CreateClauseContext(this, source, resourceType, lambdaScope);

        return _whereClauseBuilder.ApplyWhere(filter, clauseContext);
    }

    protected virtual Expression ApplySort(Expression source, SortExpression sort, ResourceType resourceType, QueryableBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(context);

        using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(context.ElementType);
        QueryClauseBuilderContext clauseContext = context.CreateClauseContext(this, source, resourceType, lambdaScope);

        return _orderClauseBuilder.ApplyOrderBy(sort, clauseContext);
    }

    protected virtual Expression ApplyPagination(Expression source, PaginationExpression pagination, ResourceType resourceType, QueryableBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(pagination);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(context);

        using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(context.ElementType);
        QueryClauseBuilderContext clauseContext = context.CreateClauseContext(this, source, resourceType, lambdaScope);

        return _skipTakeClauseBuilder.ApplySkipTake(pagination, clauseContext);
    }

    protected virtual Expression ApplySelection(Expression source, FieldSelection selection, ResourceType resourceType, QueryableBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(context);

        using LambdaScope lambdaScope = context.LambdaScopeFactory.CreateScope(context.ElementType);
        QueryClauseBuilderContext clauseContext = context.CreateClauseContext(this, source, resourceType, lambdaScope);

        return _selectClauseBuilder.ApplySelect(selection, clauseContext);
    }
}
