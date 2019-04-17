
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

    public class BeforeReadTests : ResourceHooksTestBase
    {
        public BeforeReadTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }



        [Fact]
        public void BeforeRead()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.BeforeRead(It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }



    }
}

