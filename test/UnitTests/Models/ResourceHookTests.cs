
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


namespace UnitTests.Models
{
    public class ResourceHooks_Tests
    {


        public ResourceHooks_Tests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }

        [Fact]
        public void Hook_Discovery()
        {
            // arrange & act
            var hookConfig = new ImplementedResourceHooks<Dummy>();
            // assert
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
            Assert.Equal(2, hookConfig.ImplementedHooks.Length);
        }

        IImplementedResourceHooks<TEntity> SetDiscoverableHooks<TEntity>(ResourceHook[] implementedHooks)
            where TEntity : class, IIdentifiable
        {
            var mock = new Mock<IImplementedResourceHooks<TEntity>>();
            mock.Setup(discovery => discovery.ImplementedHooks)
                .Returns(implementedHooks);
            return mock.Object;
        }
        [Fact]
        public void BeforeCreate_Hook_Is_Called_With_Nested_BeforeUpdate()
        {
            // arrange
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase<TodoItem, Person>();
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
        public void AfterCreate_Hook_Is_Called_With_Nested_AfterUpdate()
        {
            // arrange
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjectsForNestedCase<TodoItem, Person>();
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
        public void BeforeRead_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor, var x) = CreateTestObjectsForSimpleCase();
            // act
            hookExecutor.BeforeRead(It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }
        [Fact]
        public void AfterRead_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor, var x) = CreateTestObjectsForSimpleCase();
            // act
            hookExecutor.AfterRead(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeUpdate_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor, var x) = CreateTestObjectsForSimpleCase();
            // act
            hookExecutor.BeforeUpdate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterUpdate_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor, var x) = CreateTestObjectsForSimpleCase();
            // act
            hookExecutor.AfterUpdate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeDelete_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor, var x) = CreateTestObjectsForSimpleCase();

            // act
            hookExecutor.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>());

            // assert
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterDelete_Hook_Is_Called()
        {
            // arrange
            (var resourceDefinitionMock, var contextMock, var hookExecutor, var x) = CreateTestObjectsForSimpleCase();
            // act
            hookExecutor.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), true, It.IsAny<ResourceAction>());
            // assert
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<IEnumerable<TodoItem>>(), true, It.IsAny<ResourceAction>()), Times.Once());
        }


        (Mock<IResourceHookContainer<TodoItem>>, Mock<IJsonApiContext>, IResourceHookExecutor<TodoItem>, Mock<IResourceHookContainer<IIdentifiable>>) CreateTestObjectsForSimpleCase()
        {

            var discovery = new ImplementedResourceHooks<TodoItem>();
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            (var todoItemResource, var identifiableTodoItemResource) = CreateResourceDefinition<TodoItem>(discovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            // wiring up the mocked GenericProcessorFactory to return the correct resource definition
            SetupProcessorFactoryForResourceDefinition<TodoItem, TodoItem>(processorFactory, todoItemResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TodoItem>(context.Object, discovery, meta);
            return (todoItemResource, context, hookExecutor, identifiableTodoItemResource);
        }

        (Mock<IJsonApiContext> context, IResourceHookExecutor<TMain>, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<IIdentifiable>>)
            CreateTestObjectsForNestedCase<TMain, TNested>(
            IImplementedResourceHooks<TMain> mainDiscovery = null,
            IImplementedResourceHooks<TNested> nestedDiscovery = null
            )
            where TMain : class, IIdentifiable
            where TNested : class, IIdentifiable
        {
            mainDiscovery = mainDiscovery ?? new ImplementedResourceHooks<TMain>();
            nestedDiscovery = nestedDiscovery ?? new ImplementedResourceHooks<TNested>();

            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            (var mainResource, var identifiableMainResource) = CreateResourceDefinition<TMain>(mainDiscovery);
            var identifiableNestedResource = CreateResourceDefinition<TNested>(nestedDiscovery).Item2;

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            SetupProcessorFactoryForResourceDefinition<TMain, TMain>(processorFactory, mainResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TMain>(context.Object, mainDiscovery, meta);

            SetupProcessorFactoryForResourceDefinition<TNested, IIdentifiable>(processorFactory, identifiableNestedResource.Object);

            return (context, hookExecutor, mainResource, identifiableNestedResource);
        }

        Mock<IResourceHookContainer<TImplementAs>> ImplementAs<TImplementAs, TActual>(
            Mock<IResourceHookContainer<TImplementAs>> resourceDefinition,
            IImplementedResourceHooks<TActual> discovery
            ) where TImplementAs : class, IIdentifiable
                where TActual : class, IIdentifiable
        {
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.BeforeCreate(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TImplementAs>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.AfterCreate(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TImplementAs>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), null))
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.AfterRead(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TImplementAs>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TImplementAs>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.AfterUpdate(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TImplementAs>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<ResourceAction>()))
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.AfterDelete(It.IsAny<IEnumerable<TImplementAs>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()))
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TImplementAs>>()
                .Setup(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()))
                .Returns<ResourceHook>((hook) => discovery.ImplementedHooks.Contains(hook));
            resourceDefinition
                .As<IResourceHookContainer<TActual>>()
                .Setup(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()))
                .Returns<ResourceHook>((hook) => discovery.ImplementedHooks.Contains(hook));
            return resourceDefinition;
        }

        (Mock<IResourceHookContainer<TModel>>, Mock<IResourceHookContainer<IIdentifiable>>) CreateResourceDefinition
            <TModel>(IImplementedResourceHooks<TModel> discovery)
            where TModel : class, IIdentifiable
        {
            var resourceDefinition = new Mock<IResourceHookContainer<TModel>>();

            var identifiableResourceDefinition = ImplementAs(resourceDefinition.As<IResourceHookContainer<IIdentifiable>>(), discovery);
            var modelSpecificResourceDefinition = ImplementAs(resourceDefinition.As<IResourceHookContainer<TModel>>(), discovery);

            return (modelSpecificResourceDefinition, identifiableResourceDefinition);
        }



        (Mock<IJsonApiContext>, Mock<IGenericProcessorFactory>) CreateContextAndProcessorMocks()
        {
            var processorFactory = new Mock<IGenericProcessorFactory>();
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);
            return (context, processorFactory);
        }


        void SetupProcessorFactoryForResourceDefinition<TModel, TInnerContainerType>(
            Mock<IGenericProcessorFactory> processorFactory, 
            IResourceHookContainer<TInnerContainerType> modelResource)
            where TModel : class, IIdentifiable
            where TInnerContainerType : class, IIdentifiable

        {
            processorFactory.Setup(c => c.GetProcessor<IResourceHookContainer>(typeof(IResourceHookContainer<>), typeof(TModel)))
            .Returns(modelResource);
        }

    }

    public class Dummy : Identifiable
    {

    }
    public class DummyResourceDefinition : ResourceDefinition<Dummy>
    {
        public override void BeforeDelete(IEnumerable<Dummy> entities, ResourceAction actionSource)
        {
        }
        public override void AfterDelete(IEnumerable<Dummy> entities, bool succeeded, ResourceAction actionSource)
        {

        }
    }


}
