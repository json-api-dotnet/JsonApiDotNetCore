
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExample.Resources;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace UnitTests.ResourceHooks
{

    public class BeforeDeleteTests : ResourceHooksTestBase
    {
        public BeforeDeleteTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }

        [Fact]
        public void BeforeDelete()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>();
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects(discovery);

            var todoInput = new List<TodoItem>() { new TodoItem() };

            // act
            hookExecutor.BeforeDelete(todoInput, It.IsAny<ResourceAction>());

            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            resourceDefinitionMock.As<IResourceHookContainer<IIdentifiable>>().Verify(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()), Times.AtLeastOnce());
            resourceDefinitionMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void BeforeDelete_Without_Any_Hook_Implemented()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjects(discovery);

            // act
            hookExecutor.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());

            // assert
            resourceDefinitionMock.As<IResourceHookContainer<IIdentifiable>>().Verify(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()), Times.AtLeastOnce());
            resourceDefinitionMock.VerifyNoOtherCalls();
        }
    }
}

