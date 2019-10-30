using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class AfterDeleteTests : HooksTestsSetup
    {
        readonly ResourceHook[] targetHooks = { ResourceHook.AfterDelete };

        [Fact]
        public void AfterDelete()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var resourceDefinitionMock) = CreateTestObjects(discovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterDelete(todoList, ResourcePipeline.Delete, It.IsAny<bool>());

            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<HashSet<TodoItem>>(), ResourcePipeline.Delete, It.IsAny<bool>()), Times.Once());
            VerifyNoOtherCalls(resourceDefinitionMock);
        }

        [Fact]
        public void AfterDelete_Without_Any_Hook_Implemented()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var resourceDefinitionMock) = CreateTestObjects(discovery);
            var todoList = CreateTodoWithOwner();

            // act
            hookExecutor.AfterDelete(todoList, ResourcePipeline.Delete, It.IsAny<bool>());

            // assert
            VerifyNoOtherCalls(resourceDefinitionMock);
        }
    }
}

