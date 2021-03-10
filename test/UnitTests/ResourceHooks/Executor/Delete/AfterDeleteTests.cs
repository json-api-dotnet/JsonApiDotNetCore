using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Delete
{
    public sealed class AfterDeleteTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.AfterDelete
        };

        [Fact]
        public void AfterDelete()
        {
            // Arrange
            IHooksDiscovery<TodoItem> discovery = SetDiscoverableHooks<TodoItem>(_targetHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> resourceDefinitionMock) = CreateTestObjects(discovery);
            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterDelete(todoList, ResourcePipeline.Delete, It.IsAny<bool>());

            // Assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Delete, It.IsAny<bool>()), Times.Once());
            VerifyNoOtherCalls(resourceDefinitionMock);
        }

        [Fact]
        public void AfterDelete_Without_Any_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> discovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            (IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> resourceDefinitionMock) = CreateTestObjects(discovery);
            IEnumerable<TodoItem> todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterDelete(todoList, ResourcePipeline.Delete, It.IsAny<bool>());

            // Assert
            VerifyNoOtherCalls(resourceDefinitionMock);
        }
    }
}
