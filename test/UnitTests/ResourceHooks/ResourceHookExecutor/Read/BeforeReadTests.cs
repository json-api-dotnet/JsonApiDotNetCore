using System.Collections.Generic;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class BeforeReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeRead };

        [Fact]
        public void BeforeRead()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var (iqMock, hookExecutor, todoResourceMock) = CreateTestObjects(todoDiscovery);

            iqMock.Setup(c => c.Get()).Returns(new List<List<RelationshipAttribute>>());
            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, false, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock);

        }

        [Fact]
        public void BeforeReadWithInclusion()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);

            var (iqMock, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            // eg a call on api/todoItems?include=owner,assignee,stakeHolders
            iqMock.Setup(c => c.Get()).Returns(GetIncludedRelationshipsChains("owner", "assignee", "stakeHolders"));

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, DisableDbValues);

            var (iqMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            iqMock.Setup(c => c.Get()).Returns(GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders"));

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }


        [Fact]
        public void BeforeReadWithNestedInclusion_No_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, DisableDbValues);

            var (iqMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            iqMock.Setup(c => c.Get()).Returns(GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders"));

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion_No_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, DisableDbValues);

            var (iqMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            iqMock.Setup(c => c.Get()).Returns(GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders"));

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, false, null), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeReadWithNestedInclusion_No_Grandchild_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            var (iqMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            iqMock.Setup(c => c.Get()).Returns(GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders"));

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            todoResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, false, null), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(ResourcePipeline.Get, true, null), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }


        [Fact]
        public void BeforeReadWithNestedInclusion_Without_Any_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            var (iqMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            iqMock.Setup(c => c.Get()).Returns(GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders"));

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }
    }
}

