using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Read
{
    public sealed class BeforeReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks = { ResourceHook.BeforeRead };

        [Fact]
        public void BeforeRead()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var (hookExecutor, todoResourceMock) = CreateTestObjects(todoDiscovery);

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
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            var (constraintsMock, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);

            // eg a call on api/todoItems?include=owner,assignee,stakeHolders
            var relationshipsChains = GetIncludedRelationshipsChains("owner", "assignee", "stakeHolders");
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(ConvertInclusionChains(relationshipsChains).GetEnumerator());

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
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, DisableDbValues);

            var (constraintsMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            var relationshipsChains = GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders");
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(ConvertInclusionChains(relationshipsChains).GetEnumerator());

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
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, DisableDbValues);

            var (constraintsMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            var relationshipsChains = GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders");
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(ConvertInclusionChains(relationshipsChains).GetEnumerator());

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
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, DisableDbValues);

            var (constraintsMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            var relationshipsChains = GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders");
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(ConvertInclusionChains(relationshipsChains).GetEnumerator());

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
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            var (constraintsMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            var relationshipsChains = GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders");
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(ConvertInclusionChains(relationshipsChains).GetEnumerator());

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

            var (constraintsMock, hookExecutor, todoResourceMock, ownerResourceMock, passportResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, passportDiscovery);

            // eg a call on api/todoItems?include=owner.passport,assignee,stakeHolders
            var relationshipsChains = GetIncludedRelationshipsChains("owner.passport", "assignee", "stakeHolders");
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(ConvertInclusionChains(relationshipsChains).GetEnumerator());

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);
            // Assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }
    }
}

