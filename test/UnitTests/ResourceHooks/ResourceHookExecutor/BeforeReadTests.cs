using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeReadTests : ResourceHooksTestBase
    {
        public BeforeReadTests()
        {

            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .AddResource<Passport>()
                .Build();
        }

        [Fact]
        public void BeforeRead()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);
            var todoList = CreateTodoWithOwner();

            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>());
            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, false, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock);

        }

        [Fact]
        public void BeforeReadWithInclusion()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();
            var passportDiscovery = SetDiscoverableHooks<Passport>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }


        [Fact]
        public void BeforeReadWithNestedInclusion_No_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();
            var passportDiscovery = SetDiscoverableHooks<Passport>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion_No_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);
            var passportDiscovery = SetDiscoverableHooks<Passport>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, false, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion_No_Grandchild_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();
            var passportDiscovery = SetDiscoverableHooks<Passport>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourceAction.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }


        [Fact]
        public void BeforeReadWithNestedInclusion_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);
            var passportDiscovery = SetDiscoverableHooks<Passport>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourceAction.Get);
            // assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }
    }
}

