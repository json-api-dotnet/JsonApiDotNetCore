using System.ComponentModel;
using DapperExample.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace DapperExample.Definitions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, long>
{
    private readonly TimeProvider _timeProvider;

    public TodoItemDefinition(IResourceGraph resourceGraph, TimeProvider timeProvider)
        : base(resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
    }

    public override SortExpression OnApplySort(SortExpression? existingSort)
    {
        return existingSort ?? GetDefaultSortOrder();
    }

    private SortExpression GetDefaultSortOrder()
    {
        return CreateSortExpressionFromLambda([
            (todoItem => todoItem.Priority, ListSortDirection.Ascending),
            (todoItem => todoItem.LastModifiedAt, ListSortDirection.Descending)
        ]);
    }

    public override Task OnWritingAsync(TodoItem resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (writeOperation == WriteOperationKind.CreateResource)
        {
            resource.CreatedAt = _timeProvider.GetUtcNow();
        }
        else if (writeOperation == WriteOperationKind.UpdateResource)
        {
            resource.LastModifiedAt = _timeProvider.GetUtcNow();
        }

        return Task.CompletedTask;
    }
}
