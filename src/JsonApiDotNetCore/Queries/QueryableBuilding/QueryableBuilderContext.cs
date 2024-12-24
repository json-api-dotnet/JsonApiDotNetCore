using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Immutable contextual state for <see cref="IQueryableBuilder" />.
/// </summary>
[PublicAPI]
public sealed class QueryableBuilderContext
{
    /// <summary>
    /// The source expression to append to.
    /// </summary>
    public Expression Source { get; }

    /// <summary>
    /// The element type for <see cref="Source" />.
    /// </summary>
    public Type ElementType { get; }

    /// <summary>
    /// The extension type to generate calls on, typically <see cref="Queryable" /> or <see cref="Enumerable" />.
    /// </summary>
    public Type ExtensionType { get; }

    /// <summary>
    /// The Entity Framework Core entity model.
    /// </summary>
    public IReadOnlyModel EntityModel { get; }

    /// <summary>
    /// Used to produce unique names for lambda parameters.
    /// </summary>
    public LambdaScopeFactory LambdaScopeFactory { get; }

    /// <summary>
    /// Enables to pass custom state that you'd like to transfer between calls.
    /// </summary>
    public object? State { get; }

    public QueryableBuilderContext(Expression source, Type elementType, Type extensionType, IReadOnlyModel entityModel, LambdaScopeFactory lambdaScopeFactory,
        object? state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(elementType);
        ArgumentNullException.ThrowIfNull(extensionType);
        ArgumentNullException.ThrowIfNull(entityModel);
        ArgumentNullException.ThrowIfNull(lambdaScopeFactory);

        Source = source;
        ElementType = elementType;
        ExtensionType = extensionType;
        EntityModel = entityModel;
        LambdaScopeFactory = lambdaScopeFactory;
        State = state;
    }

    public static QueryableBuilderContext CreateRoot(IQueryable source, Type extensionType, IReadOnlyModel entityModel, object? state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(extensionType);
        ArgumentNullException.ThrowIfNull(entityModel);

        var lambdaScopeFactory = new LambdaScopeFactory();

        return new QueryableBuilderContext(source.Expression, source.ElementType, extensionType, entityModel, lambdaScopeFactory, state);
    }

    public QueryClauseBuilderContext CreateClauseContext(IQueryableBuilder queryableBuilder, Expression source, ResourceType resourceType,
        LambdaScope lambdaScope)
    {
        ArgumentNullException.ThrowIfNull(queryableBuilder);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(lambdaScope);

        return new QueryClauseBuilderContext(source, resourceType, ExtensionType, EntityModel, LambdaScopeFactory, lambdaScope, queryableBuilder, State);
    }
}
