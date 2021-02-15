using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor
{
    public sealed class SameResourceTypeTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks = { ResourceHook.OnReturn };

        [Fact]
        public void Resource_Has_Multiple_Relations_To_Same_Type()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var person1 = new Person();
            var todo = new TodoItem { Owner = person1 };
            var person2 = new Person { AssignedTodoItems = new HashSet<TodoItem> { todo } };
            todo.Assignee = person2;
            var person3 = new Person { StakeHolderTodoItem = todo };
            todo.StakeHolders = new HashSet<Person> { person3 };
            var todoList = new List<TodoItem> { todo };

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void Resource_Has_Cyclic_Relations()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var (_, hookExecutor, todoResourceMock) = CreateTestObjects(todoDiscovery);
            var todo = new TodoItem();
            todo.ParentTodo  = todo;
            todo.ChildTodoItems = new List<TodoItem> { todo };
            var todoList = new List<TodoItem> { todo };

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock);
        }

        [Fact]
        public void Resource_Has_Nested_Cyclic_Relations()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var (_, hookExecutor, todoResourceMock) = CreateTestObjects(todoDiscovery);
            var rootTodo = new TodoItem { Id = 1 };
            var child = new TodoItem { ParentTodo  = rootTodo, Id = 2 };
            rootTodo.ChildTodoItems = new List<TodoItem> { child };
            var grandChild = new TodoItem { ParentTodo  = child, Id = 3 };
            child.ChildTodoItems = new List<TodoItem> { grandChild };
            var greatGrandChild = new TodoItem { ParentTodo  = grandChild, Id = 4 };
            grandChild.ChildTodoItems = new List<TodoItem> { greatGrandChild };
            greatGrandChild.ChildTodoItems = new List<TodoItem> { rootTodo };
            var todoList = new List<TodoItem> { rootTodo };

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Exactly(4));
            VerifyNoOtherCalls(todoResourceMock);
        }
    }
}
