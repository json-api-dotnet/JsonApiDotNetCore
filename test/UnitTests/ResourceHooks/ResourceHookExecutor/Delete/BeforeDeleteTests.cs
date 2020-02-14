using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeDeleteTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeDelete };

        [Fact]
        public void BeforeDelete()
        {
            // Arrange
            var discovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            (var _, var hookExecutor, var resourceDefinitionMock) = CreateTestObjects(discovery);

            var todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(todoList, ResourcePipeline.Delete);

            // Assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEntityHashSet<TodoItem>>(), It.IsAny<ResourcePipeline>()), Times.Once());
            resourceDefinitionMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void BeforeDelete_Without_Any_Hook_Implemented()
        {
            // Arrange
            var discovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            (var _, var hookExecutor, var resourceDefinitionMock) = CreateTestObjects(discovery);

            var todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(todoList, ResourcePipeline.Delete);

            // Assert
            resourceDefinitionMock.VerifyNoOtherCalls();
        }
    }
}

