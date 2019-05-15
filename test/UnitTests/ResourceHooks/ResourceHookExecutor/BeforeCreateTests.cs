using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeCreateTests : HooksTestsSetup
    {
        [Fact]
        public void BeforeCreate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, NoHooks);
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooksNoImplicit, NoHooks);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(It.IsAny<IEnumerable<string>>(), It.IsAny<IUpdatedRelationshipHelper<Person>>(), ResourceAction.Create), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks);
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooksNoImplicit, NoHooks);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(It.IsAny<IEnumerable<string>>(), It.IsAny<IUpdatedRelationshipHelper<Person>>(), ResourceAction.Create), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, NoHooks);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
        [Fact]
        public void BeforeCreate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
            // assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}

