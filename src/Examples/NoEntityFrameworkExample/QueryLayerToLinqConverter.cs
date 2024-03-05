using System.Collections;
using System.Linq.Expressions;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoEntityFrameworkExample;

internal sealed class QueryLayerToLinqConverter(IReadOnlyModel entityModel, IQueryableBuilder queryableBuilder)
{
    private readonly IReadOnlyModel _entityModel = entityModel;
    private readonly IQueryableBuilder _queryableBuilder = queryableBuilder;

    public IEnumerable<TResource> ApplyQueryLayer<TResource>(QueryLayer queryLayer, IEnumerable<TResource> resources)
        where TResource : class, IIdentifiable
    {
        // The Include() extension method from Entity Framework Core is unavailable, so rewrite into selectors.
        var converter = new QueryLayerIncludeConverter(queryLayer);
        converter.ConvertIncludesToSelections();

        // Convert QueryLayer into LINQ expression.
        IQueryable source = ((IEnumerable)resources).AsQueryable();
        var context = QueryableBuilderContext.CreateRoot(source, typeof(Enumerable), _entityModel, null);
        Expression expression = _queryableBuilder.ApplyQuery(queryLayer, context);

        // Insert null checks to prevent a NullReferenceException during execution of expressions such as:
        // 'todoItems => todoItems.Where(todoItem => todoItem.Assignee.Id == 1)' when a TodoItem doesn't have an assignee.
        NullSafeExpressionRewriter rewriter = new();
        expression = rewriter.Rewrite(expression);

        // Compile and execute LINQ expression against the in-memory database.
        Delegate function = Expression.Lambda(expression).Compile();
        object result = function.DynamicInvoke()!;
        return (IEnumerable<TResource>)result;
    }
}
