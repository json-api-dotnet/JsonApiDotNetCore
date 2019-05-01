using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class AfterCreateTests : ResourceHooksTestBase
    {
        public AfterCreateTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }

        [Fact]
        public void AfterCreate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();
            // act
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());

            todoResourceMock.VerifyNoOtherCalls();
            ownerResourceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AfterCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());
            ownerResourceMock.VerifyNoOtherCalls();
            todoResourceMock.VerifyNoOtherCalls();

        }

        [Fact]
        public void AfterCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
            todoResourceMock.VerifyNoOtherCalls();
            ownerResourceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AfterCreate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            todoResourceMock.VerifyNoOtherCalls();
            ownerResourceMock.VerifyNoOtherCalls();
        }
    }
}

