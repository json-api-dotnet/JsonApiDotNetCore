using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeCreateTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeCreate, ResourceHook.BeforeUpdateRelationship };

        [Fact]
        public void BeforeCreate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IAffectedResources<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IAffectedRelationships<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IAffectedResources<TodoItem>>(), ResourcePipeline.Post), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IAffectedRelationships<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IAffectedResources<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
        [Fact]
        public void BeforeCreate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);
            // assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}

