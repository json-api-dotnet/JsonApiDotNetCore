using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Read
{
    public sealed class BeforeReadTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.BeforeRead
        };

        [Fact]
        public void BeforeRead()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock) = CreateTestObjects(todoDiscovery);

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);

            (Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, _, IResourceHookExecutor hookExecutor,
                    Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Person>> ownerResourceMock) =
                CreateTestObjects(todoDiscovery, personDiscovery);

            IEnumerable<IQueryConstraintProvider> constraintProviders = Wrap(ToIncludeExpression("owner", "assignee", "stakeHolders"));
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(constraintProviders.GetEnumerator());

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, DisableDbValues);

            (Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, IResourceHookExecutor hookExecutor,
                Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Person>> ownerResourceMock,
                Mock<IResourceHookContainer<Passport>> passportResourceMock) = CreateTestObjectsC(todoDiscovery, personDiscovery, passportDiscovery);

            IEnumerable<IQueryConstraintProvider> constraintProviders = Wrap(ToIncludeExpression("owner.passport", "assignee", "stakeHolders"));
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(constraintProviders.GetEnumerator());

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, DisableDbValues);

            (Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, IResourceHookExecutor hookExecutor,
                Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Person>> ownerResourceMock,
                Mock<IResourceHookContainer<Passport>> passportResourceMock) = CreateTestObjectsC(todoDiscovery, personDiscovery, passportDiscovery);

            IEnumerable<IQueryConstraintProvider> constraintProviders = Wrap(ToIncludeExpression("owner.passport", "assignee", "stakeHolders"));
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(constraintProviders.GetEnumerator());

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, DisableDbValues);

            (Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, IResourceHookExecutor hookExecutor,
                Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Person>> ownerResourceMock,
                Mock<IResourceHookContainer<Passport>> passportResourceMock) = CreateTestObjectsC(todoDiscovery, personDiscovery, passportDiscovery);

            IEnumerable<IQueryConstraintProvider> constraintProviders = Wrap(ToIncludeExpression("owner.passport", "assignee", "stakeHolders"));
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(constraintProviders.GetEnumerator());

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, DisableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            (Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, IResourceHookExecutor hookExecutor,
                Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Person>> ownerResourceMock,
                Mock<IResourceHookContainer<Passport>> passportResourceMock) = CreateTestObjectsC(todoDiscovery, personDiscovery, passportDiscovery);

            IEnumerable<IQueryConstraintProvider> constraintProviders = Wrap(ToIncludeExpression("owner.passport", "assignee", "stakeHolders"));
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(constraintProviders.GetEnumerator());

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            (Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, IResourceHookExecutor hookExecutor,
                Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Person>> ownerResourceMock,
                Mock<IResourceHookContainer<Passport>> passportResourceMock) = CreateTestObjectsC(todoDiscovery, personDiscovery, passportDiscovery);

            IEnumerable<IQueryConstraintProvider> constraintProviders = Wrap(ToIncludeExpression("owner.passport", "assignee", "stakeHolders"));
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(constraintProviders.GetEnumerator());

            // Act
            hookExecutor.BeforeRead<TodoItem>(ResourcePipeline.Get);

            // Assert
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock, passportResourceMock);
        }
    }
}
