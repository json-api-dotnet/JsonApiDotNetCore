using System.Collections.Generic;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor
{
    public sealed class SameResourceTypeTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.OnReturn
        };

        [Fact]
        public void Resource_Has_Multiple_Relations_To_Same_Type()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            var person1 = new Person();

            var todo = new TodoItem
            {
                Owner = person1
            };

            var person2 = new Person
            {
                AssignedTodoItems = new HashSet<TodoItem>
                {
                    todo
                }
            };

            todo.Assignee = person2;

            var person3 = new Person
            {
                StakeHolderTodoItem = todo
            };

            todo.StakeHolders = new HashSet<Person>
            {
                person3
            };

            List<TodoItem> todoList = todo.AsList();

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock) = CreateTestObjects(todoDiscovery);
            var todo = new TodoItem();
            todo.ParentTodo = todo;
            todo.ChildTodoItems = todo.AsList();
            List<TodoItem> todoList = todo.AsList();

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock) = CreateTestObjects(todoDiscovery);

            var rootTodo = new TodoItem
            {
                Id = 1
            };

            var child = new TodoItem
            {
                ParentTodo = rootTodo,
                Id = 2
            };

            rootTodo.ChildTodoItems = child.AsList();

            var grandChild = new TodoItem
            {
                ParentTodo = child,
                Id = 3
            };

            child.ChildTodoItems = grandChild.AsList();

            var greatGrandChild = new TodoItem
            {
                ParentTodo = grandChild,
                Id = 4
            };

            grandChild.ChildTodoItems = greatGrandChild.AsList();
            greatGrandChild.ChildTodoItems = rootTodo.AsList();
            List<TodoItem> todoList = rootTodo.AsList();

            // Act
            hookExecutor.OnReturn(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Exactly(4));
            VerifyNoOtherCalls(todoResourceMock);
        }
    }
}
