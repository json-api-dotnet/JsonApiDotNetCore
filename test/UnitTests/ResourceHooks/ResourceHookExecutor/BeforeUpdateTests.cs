using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeUpdateTests : ResourceHooksTestBase
    {
        public BeforeUpdateTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }

        [Fact]
        public void BeforeUpdate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<TodoItem>>(), ResourceAction.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<Person>>(), ResourceAction.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<Person>>(), ResourceAction.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<TodoItem>>(), ResourceAction.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }
    }
}

