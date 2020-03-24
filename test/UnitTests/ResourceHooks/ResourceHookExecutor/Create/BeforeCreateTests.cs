using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class BeforeCreateTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeCreate, ResourceHook.BeforeUpdateRelationship };

        [Fact]
        public void BeforeCreate()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);

            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEntityHashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);

            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEntityHashSet<TodoItem>>(), ResourcePipeline.Post), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEntityHashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
        [Fact]
        public void BeforeCreate_Without_Any_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // Assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}

