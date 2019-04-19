
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

    public class AfterDeleteTests : ResourceHooksTestBase
    {
        public AfterDeleteTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }


        [Fact]
        public void AfterDelete()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>();
            (var contextMock, var hookExecutor, var resourceDefinitionMock) = CreateTestObjects(discovery);
            // act
            hookExecutor.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()), Times.Once());
            resourceDefinitionMock.As<IResourceHookContainer<IIdentifiable>>().Verify(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()), Times.AtLeastOnce());
            resourceDefinitionMock.VerifyNoOtherCalls();

        }

        [Fact]
        public void AfterDelete_Without_Any_Hook_Implemented()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            (var contextMock, var hookExecutor, var resourceDefinitionMock) = CreateTestObjects(discovery);
            // act
            hookExecutor.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.As<IResourceHookContainer<IIdentifiable>>().Verify(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()), Times.AtLeastOnce());
            resourceDefinitionMock.VerifyNoOtherCalls();
        }
    }
}

