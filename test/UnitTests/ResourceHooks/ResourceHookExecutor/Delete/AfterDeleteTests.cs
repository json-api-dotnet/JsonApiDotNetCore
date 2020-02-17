using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class AfterDeleteTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.AfterDelete };

        [Fact]
        public void AfterDelete()
        {
            // Arrange
            var discovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var (_, hookExecutor, resourceDefinitionMock) = CreateTestObjects(discovery);
            var todoList = CreateTodoWithOwner();

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
            var discovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var (_, hookExecutor, resourceDefinitionMock) = CreateTestObjects(discovery);
            var todoList = CreateTodoWithOwner();

            // Act
            hookExecutor.AfterDelete(todoList, ResourcePipeline.Delete, It.IsAny<bool>());

            // Assert
            VerifyNoOtherCalls(resourceDefinitionMock);
        }
    }
}

