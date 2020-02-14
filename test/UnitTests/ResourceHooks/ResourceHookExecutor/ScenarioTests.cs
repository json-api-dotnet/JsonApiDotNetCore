using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class SameEntityTypeTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.OnReturn };

        [Fact]
        public void Entity_Has_Multiple_Relations_To_Same_Type()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var person1 = new Person();
            var todo = new TodoItem { Owner = person1 };
            var person2 = new Person { AssignedTodoItems = new List<TodoItem> { todo } };
            todo.Assignee = person2;
            var person3 = new Person { StakeHolderTodoItem = todo };
            todo.StakeHolders = new List<Person> { person3 };
            var todoList = new List<TodoItem> { todo };

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void Entity_Has_Cyclic_Relations()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);
            var todo = new TodoItem();
            todo.ParentTodo  = todo;
            todo.ChildrenTodos = new List<TodoItem> { todo };
            var todoList = new List<TodoItem> { todo };

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock);
        }

        [Fact]
        public void Entity_Has_Nested_Cyclic_Relations()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);
            var rootTodo = new TodoItem { Id = 1 };
            var child = new TodoItem { ParentTodo  = rootTodo, Id = 2 };
            rootTodo.ChildrenTodos = new List<TodoItem> { child };
            var grandChild = new TodoItem { ParentTodo  = child, Id = 3 };
            child.ChildrenTodos = new List<TodoItem> { grandChild };
            var greatGrandChild = new TodoItem { ParentTodo  = grandChild, Id = 4 };
            grandChild.ChildrenTodos = new List<TodoItem> { greatGrandChild };
            greatGrandChild.ChildrenTodos = new List<TodoItem> { rootTodo };
            var todoList = new List<TodoItem> { rootTodo };

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Exactly(4));
            VerifyNoOtherCalls(todoResourceMock);
        }
    }
}

