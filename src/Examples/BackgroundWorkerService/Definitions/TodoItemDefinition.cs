using System.ComponentModel;
using BackgroundWorkerService.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace BackgroundWorkerService.Definitions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, int>
{
    private readonly ILogger<TodoItemDefinition> _logger;

    public TodoItemDefinition(IResourceGraph resourceGraph, ILogger<TodoItemDefinition> logger)
        : base(resourceGraph)
    {
        _logger = logger;
    }

    public override SortExpression OnApplySort(SortExpression? existingSort)
    {
        _logger.LogInformation("Applying custom sort, based on business rules...");

        return existingSort ?? GetDefaultSortOrder();
    }

    private SortExpression GetDefaultSortOrder()
    {
        return CreateSortExpressionFromLambda(new PropertySortOrder
        {
            (todoItem => todoItem.Priority, ListSortDirection.Descending),
            (todoItem => todoItem.LastModifiedAt, ListSortDirection.Descending)
        });
    }
}
