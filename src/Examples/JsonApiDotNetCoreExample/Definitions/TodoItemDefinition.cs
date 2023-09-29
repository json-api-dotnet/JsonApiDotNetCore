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
public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, long>
{
    private readonly Func<DateTimeOffset> _getUtcNow;

#if NET6_0
    public TodoItemDefinition(IResourceGraph resourceGraph, ISystemClock systemClock)
        : base(resourceGraph)
    {
        _getUtcNow = () => systemClock.UtcNow;
    }
#else
    public TodoItemDefinition(IResourceGraph resourceGraph, TimeProvider timeProvider)
        : base(resourceGraph)
    {
        _getUtcNow = timeProvider.GetUtcNow;
    }
#endif

    public override SortExpression OnApplySort(SortExpression? existingSort)
    {
        return existingSort ?? GetDefaultSortOrder();
    }

    private SortExpression GetDefaultSortOrder()
    {
        return CreateSortExpressionFromLambda(new PropertySortOrder
        {
            (todoItem => todoItem.Priority, ListSortDirection.Ascending),
            (todoItem => todoItem.LastModifiedAt, ListSortDirection.Descending)
        });
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
