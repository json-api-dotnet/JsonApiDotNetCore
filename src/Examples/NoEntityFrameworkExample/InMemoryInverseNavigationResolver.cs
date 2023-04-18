using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample;

internal sealed class InMemoryInverseNavigationResolver : IInverseNavigationResolver
{
    private readonly IResourceGraph _resourceGraph;

    public InMemoryInverseNavigationResolver(IResourceGraph resourceGraph)
    {
        _resourceGraph = resourceGraph;
    }

    /// <inheritdoc />
    public void Resolve()
    {
        ResourceType todoItemType = _resourceGraph.GetResourceType<TodoItem>();
        RelationshipAttribute todoItemOwnerRelationship = todoItemType.GetRelationshipByPropertyName(nameof(TodoItem.Owner));
        RelationshipAttribute todoItemAssigneeRelationship = todoItemType.GetRelationshipByPropertyName(nameof(TodoItem.Assignee));
        RelationshipAttribute todoItemTagsRelationship = todoItemType.GetRelationshipByPropertyName(nameof(TodoItem.Tags));

        ResourceType personType = _resourceGraph.GetResourceType<Person>();
        RelationshipAttribute personOwnedTodoItemsRelationship = personType.GetRelationshipByPropertyName(nameof(Person.OwnedTodoItems));
        RelationshipAttribute personAssignedTodoItemsRelationship = personType.GetRelationshipByPropertyName(nameof(Person.AssignedTodoItems));

        ResourceType tagType = _resourceGraph.GetResourceType<Tag>();
        RelationshipAttribute tagTodoItemsRelationship = tagType.GetRelationshipByPropertyName(nameof(Tag.TodoItems));

        // Inverse navigations are required for pagination on non-primary endpoints.
        todoItemOwnerRelationship.InverseNavigationProperty = personOwnedTodoItemsRelationship.Property;
        todoItemAssigneeRelationship.InverseNavigationProperty = personAssignedTodoItemsRelationship.Property;
        todoItemTagsRelationship.InverseNavigationProperty = tagTodoItemsRelationship.Property;

        personOwnedTodoItemsRelationship.InverseNavigationProperty = todoItemOwnerRelationship.Property;
        personAssignedTodoItemsRelationship.InverseNavigationProperty = todoItemAssigneeRelationship.Property;

        tagTodoItemsRelationship.InverseNavigationProperty = todoItemTagsRelationship.Property;
    }
}
