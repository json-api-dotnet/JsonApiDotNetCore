using System.ComponentModel;
using DapperExample.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace DapperExample.Definitions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, long>
{
    private readonly IClock _clock;

    public TodoItemDefinition(IResourceGraph resourceGraph, IClock clock)
        : base(resourceGraph)
    {
        ArgumentGuard.NotNull(clock);

        _clock = clock;
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
            resource.CreatedAt = _clock.UtcNow;
        }
        else if (writeOperation == WriteOperationKind.UpdateResource)
        {
            resource.LastModifiedAt = _clock.UtcNow;
        }

        return Task.CompletedTask;
    }
}
