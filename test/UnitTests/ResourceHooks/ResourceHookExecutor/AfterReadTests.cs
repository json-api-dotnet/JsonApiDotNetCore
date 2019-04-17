
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

    public class AfterReadTests : ResourceHooksTestBase
    {
        public AfterReadTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }

        public void AfterRead()
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
            hookExecutor.AfterRead(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterRead(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void AfterRead_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterRead(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterRead(todoInput, It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }


        [Fact]
        public void AfterRead_Without_Child_Before_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[] { ResourceHook.AfterRead });

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterRead(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterRead(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Never());
        }
        [Fact]
        public void AfterRead_Without_Child_After_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[] { ResourceHook.BeforeRead });

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterRead(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterRead(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void AfterRead_Without_Any_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterRead(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterRead(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Never());
        }
        [Fact]
        public void AfterRead_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterRead(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterRead(todoInput, It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Never());
        }
    }
}

