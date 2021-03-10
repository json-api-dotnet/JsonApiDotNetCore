using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Delete
{
    public sealed class BeforeDeleteTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.BeforeDelete
        };

        [Fact]
        public void BeforeDelete()
        {
            // Arrange
            IHooksDiscovery<TodoItem> discovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> resourceDefinitionMock) = CreateTestObjects(discovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(todoList, ResourcePipeline.Delete);

            // Assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<IResourceHashSet<TodoItem>>(), It.IsAny<ResourcePipeline>()), Times.Once());
            resourceDefinitionMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void BeforeDelete_Without_Any_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> discovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> resourceDefinitionMock) = CreateTestObjects(discovery);

            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(todoList, ResourcePipeline.Delete);

            // Assert
            resourceDefinitionMock.VerifyNoOtherCalls();
        }
    }
}
