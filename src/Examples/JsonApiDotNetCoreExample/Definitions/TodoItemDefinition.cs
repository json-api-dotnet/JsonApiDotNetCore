using System.ComponentModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.Models;
#if NET6_0
using Microsoft.AspNetCore.Authentication;
#endif

namespace JsonApiDotNetCoreExample.Definitions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemDefinition(
    IResourceGraph resourceGraph,
#if NET6_0
    ISystemClock systemClock
#else
    TimeProvider timeProvider
#endif
) : JsonApiResourceDefinition<TodoItem, long>(resourceGraph)
{
#if NET6_0
    private readonly Func<DateTimeOffset> _getUtcNow = () => systemClock.UtcNow;
#else
    private readonly Func<DateTimeOffset> _getUtcNow = timeProvider.GetUtcNow;
#endif

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
            resource.CreatedAt = _getUtcNow();
        }
        else if (writeOperation == WriteOperationKind.UpdateResource)
        {
            resource.LastModifiedAt = _getUtcNow();
        }

        return Task.CompletedTask;
    }
}
