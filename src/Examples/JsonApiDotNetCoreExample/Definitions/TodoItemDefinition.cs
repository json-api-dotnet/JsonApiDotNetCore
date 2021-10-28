using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExample.Definitions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, int>
    {
        private readonly ISystemClock _systemClock;

        public TodoItemDefinition(IResourceGraph resourceGraph, ISystemClock systemClock)
            : base(resourceGraph)
        {
            _systemClock = systemClock;
        }

        public override SortExpression OnApplySort(SortExpression existingSort)
        {
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

        public override Task OnWritingAsync(TodoItem resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation == WriteOperationKind.CreateResource)
            {
                resource.CreatedAt = _systemClock.UtcNow;
            }
            else if (writeOperation == WriteOperationKind.UpdateResource)
            {
                resource.LastModifiedAt = _systemClock.UtcNow;
            }

            return Task.CompletedTask;
        }
    }
}
