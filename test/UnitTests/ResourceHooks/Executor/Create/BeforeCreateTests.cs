using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Create
{
    public sealed class BeforeCreateTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.BeforeCreate,
            ResourceHook.BeforeUpdateRelationship
        };

        [Fact]
        public void BeforeCreate()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IResourceHashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Post),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IResourceHashSet<TodoItem>>(), ResourcePipeline.Post), Times.Never());

            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Post),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IResourceHashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Any_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}
