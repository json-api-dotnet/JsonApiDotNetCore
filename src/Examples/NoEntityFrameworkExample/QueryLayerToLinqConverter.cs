using System.Collections;
using System.Linq.Expressions;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Internal.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NoEntityFrameworkExample;

internal sealed class QueryLayerToLinqConverter
{
    private readonly IResourceFactory _resourceFactory;
    private readonly IModel _model;

    public QueryLayerToLinqConverter(IResourceFactory resourceFactory, IModel model)
    {
        _resourceFactory = resourceFactory;
        _model = model;
    }

    public IEnumerable<TResource> ApplyQueryLayer<TResource>(QueryLayer queryLayer, IEnumerable<TResource> resources)
        where TResource : class, IIdentifiable
    {
        // The Include() extension method from Entity Framework Core is unavailable, so rewrite into selectors.
        var converter = new QueryLayerIncludeConverter(queryLayer);
        converter.ConvertIncludesToSelections();

        // Convert QueryLayer into LINQ expression.
        Expression source = ((IEnumerable)resources).AsQueryable().Expression;
        var nameFactory = new LambdaParameterNameFactory();
        var queryableBuilder = new QueryableBuilder(source, queryLayer.ResourceType.ClrType, typeof(Enumerable), nameFactory, _resourceFactory, _model);
        Expression expression = queryableBuilder.ApplyQuery(queryLayer);

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
