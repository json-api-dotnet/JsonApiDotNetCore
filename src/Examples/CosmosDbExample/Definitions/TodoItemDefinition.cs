using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CosmosDbExample.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;

#pragma warning disable AV2310 // Code block should not contain inline comment

namespace CosmosDbExample.Definitions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, Guid>
    {
        private readonly ISystemClock _systemClock;

        public TodoItemDefinition(IResourceGraph resourceGraph, ISystemClock systemClock)
            : base(resourceGraph)
        {
            _systemClock = systemClock;
        }

        /// <inheritdoc />
        public override SortExpression OnApplySort(SortExpression? existingSort)
        {
            return existingSort ?? GetDefaultSortOrder();
        }

        private SortExpression GetDefaultSortOrder()
        {
            // Cosmos DB, we would have to define a composite index in order to support a composite sort expression.
            // Therefore, we will only sort on a single property.
            return CreateSortExpressionFromLambda(new PropertySortOrder
            {
                (todoItem => todoItem.Priority, ListSortDirection.Descending)
            });
        }

        /// <inheritdoc />
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
