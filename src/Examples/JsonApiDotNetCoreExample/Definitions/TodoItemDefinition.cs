using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExample.Definitions;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, long>
{
    private readonly IResourceGraph _resourceGraph;
    private readonly ISystemClock _systemClock;

    public TodoItemDefinition(IResourceGraph resourceGraph, ISystemClock systemClock)
        : base(resourceGraph)
    {
        _resourceGraph = resourceGraph;
        _systemClock = systemClock;
    }

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
        ResourceType resourceType = _resourceGraph.GetResourceType<TodoItem>();
        RelationshipAttribute assigneeRelationship = resourceType.GetRelationshipByPropertyName(nameof(TodoItem.Assignee));
        var userDefinedCapabilities = assigneeRelationship.Property.GetCustomAttribute<UserDefinedCapabilitiesAttribute>();

        if (userDefinedCapabilities != null)
        {
            bool allowCreate = userDefinedCapabilities.AllowCreateRelationship;
            bool allowUpdate = userDefinedCapabilities.AllowUpdateRelationship;

            // custom validation here...
            Console.WriteLine($"allowCreate = {allowCreate}, allowUpdate={allowUpdate}");
        }

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
