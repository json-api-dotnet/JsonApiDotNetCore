using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <summary>
/// Immutable contextual state for *ClauseBuilder types.
/// </summary>
[PublicAPI]
public sealed class QueryClauseBuilderContext
{
    /// <summary>
    /// The source expression to append to.
    /// </summary>
    public Expression Source { get; }

    /// <summary>
    /// The resource type for <see cref="Source" />.
    /// </summary>
    public ResourceType ResourceType { get; }

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
    /// The lambda expression currently in scope.
    /// </summary>
    public LambdaScope LambdaScope { get; }

    /// <summary>
    /// The outer driver for building query clauses.
    /// </summary>
    public IQueryableBuilder QueryableBuilder { get; }

    /// <summary>
    /// Enables passing custom state that you'd like to transfer between calls.
    /// </summary>
    public object? State { get; }

    public QueryClauseBuilderContext(Expression source, ResourceType resourceType, Type extensionType, IReadOnlyModel entityModel,
        LambdaScopeFactory lambdaScopeFactory, LambdaScope lambdaScope, IQueryableBuilder queryableBuilder, object? state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentNullException.ThrowIfNull(extensionType);
        ArgumentNullException.ThrowIfNull(entityModel);
        ArgumentNullException.ThrowIfNull(lambdaScopeFactory);
        ArgumentNullException.ThrowIfNull(lambdaScope);
        ArgumentNullException.ThrowIfNull(queryableBuilder);
        AssertSameType(source.Type, resourceType);

        Source = source;
        ResourceType = resourceType;
        LambdaScope = lambdaScope;
        EntityModel = entityModel;
        ExtensionType = extensionType;
        LambdaScopeFactory = lambdaScopeFactory;
        QueryableBuilder = queryableBuilder;
        State = state;
    }

    private static void AssertSameType(Type sourceType, ResourceType resourceType)
    {
        Type? sourceElementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        if (sourceElementType != resourceType.ClrType)
        {
            throw new InvalidOperationException(
                $"Internal error: Mismatch between expression type '{sourceElementType?.Name}' and resource type '{resourceType.ClrType.Name}'.");
        }
    }

    public QueryClauseBuilderContext WithSource(Expression source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new QueryClauseBuilderContext(source, ResourceType, ExtensionType, EntityModel, LambdaScopeFactory, LambdaScope, QueryableBuilder, State);
    }

    public QueryClauseBuilderContext WithLambdaScope(LambdaScope lambdaScope)
    {
        ArgumentNullException.ThrowIfNull(lambdaScope);

        return new QueryClauseBuilderContext(Source, ResourceType, ExtensionType, EntityModel, LambdaScopeFactory, lambdaScope, QueryableBuilder, State);
    }
}
