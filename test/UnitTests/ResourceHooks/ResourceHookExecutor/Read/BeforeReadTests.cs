using System.Collections.Generic;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeRead };

        [Fact]
        public void BeforeRead()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);
            var todoList = CreateTodoWithOwner();

            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>());
            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, false, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock);

        }

        [Fact]
        public void BeforeReadWithInclusion()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }


        [Fact]
        public void BeforeReadWithNestedInclusion_No_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion_No_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, false, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion_No_Grandchild_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Read, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }


        [Fact]
        public void BeforeReadWithNestedInclusion_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock, var passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);
            var todoList = CreateTodoWithOwner();

            // eg a call on api/todo-items?include=owner.passport,assignee,stake-holders
            contextMock.Setup(c => c.IncludedRelationships).Returns(new List<string>() { "owner.passport", "assignee", "stake-holders" });

            // act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Read);
            // assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }
    }
}

