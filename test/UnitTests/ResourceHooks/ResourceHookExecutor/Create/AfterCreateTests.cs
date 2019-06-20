using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class AfterCreateTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.AfterCreate, ResourceHook.AfterUpdateRelationship };

        [Fact]
        public void AfterCreate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(todoList, ResourcePipeline.Post);

            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdateRelationship(It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void AfterCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(todoList, ResourcePipeline.Post);

            // assert
            ownerResourceMock.Verify(rd => rd.AfterUpdateRelationship(It.IsAny<IRelationshipsDictionary<Person>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void AfterCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(todoList, ResourcePipeline.Post);

            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void AfterCreate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(todoList, ResourcePipeline.Post);

            // assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}

