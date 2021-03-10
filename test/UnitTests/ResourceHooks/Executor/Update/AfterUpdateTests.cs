using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Update
{
    public sealed class AfterUpdateTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.AfterUpdate,
            ResourceHook.AfterUpdateRelationship
        };

        [Fact]
        public void AfterUpdate()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterUpdate(todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdateRelationship(It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void AfterUpdate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterUpdate(todoList, ResourcePipeline.Patch);

            // Assert
            ownerResourceMock.Verify(rd => rd.AfterUpdateRelationship(It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void AfterUpdate_Without_Child_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterUpdate(todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void AfterUpdate_Without_Any_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterUpdate(todoList, ResourcePipeline.Patch);

            // Assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}
