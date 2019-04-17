
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

    public class ResourceHookExectuorTests : ResourceHooksTestBase
    {
        public ResourceHookExectuorTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }


        [Fact]
        public void BeforeCreate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.BeforeCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.BeforeCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(todoInput, It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.BeforeCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
        }
        [Fact]
        public void BeforeCreate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.BeforeCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(todoInput, It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
        }

        [Fact]
        public void AfterCreate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void AfterCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(todoInput, It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void AfterCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
        }

        [Fact]
        public void AfterCreate_Without_Any_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterCreate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(todoInput, It.IsAny<ResourceAction>()), Times.Never());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Never());
        }

        [Fact]
        public void BeforeRead()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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
        [Fact]
        public void AfterRead()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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
            ownerResourceMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>() ), Times.Once());
        }

        [Fact]
        public void AfterRead_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
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

        [Fact]
        public void BeforeUpdate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.BeforeUpdate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterUpdate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase(todoDiscovery, personDiscovery);
            var todoInput = new List<TodoItem>() { new TodoItem
                {
                    Owner = new Person()
                }
            };
            // act
            hookExecutor.AfterUpdate(todoInput, It.IsAny<ResourceAction>());
            // assert
            todoResourceMock.Verify(rd => rd.AfterUpdate(todoInput, It.IsAny<ResourceAction>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<IIdentifiable>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeDelete()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>();
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjectsForSimpleCase(discovery);

            // act
            hookExecutor.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());

            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void BeforeDelete_Without_Any_Hook_Implemented()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjectsForSimpleCase(discovery);

            // act
            hookExecutor.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());

            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>()), Times.Never());
        }


        [Fact]
        public void AfterDelete()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>();
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjectsForSimpleCase(discovery);
            // act
            hookExecutor.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void AfterDelete_Without_Any_Hook_Implemented()
        {
            // arrange
            var discovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
            (var resourceDefinitionMock, var contextMock, var hookExecutor) = CreateTestObjectsForSimpleCase(discovery);
            // act
            hookExecutor.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()), Times.Never());
        }


    }
}

