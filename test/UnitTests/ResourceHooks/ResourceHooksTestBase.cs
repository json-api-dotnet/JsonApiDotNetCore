
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

    public class ResourceHooksTestBase
    {
        protected (Mock<IResourceHookContainer<TodoItem>>, Mock<IJsonApiContext>, IResourceHookExecutor<TodoItem>) 
        CreateTestObjectsForSimpleCase(IImplementedResourceHooks<TodoItem> discovery)
        {

            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            (var todoItemResource, var identifiableTodoItemResource) = CreateResourceDefinition(discovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            // wiring up the mocked GenericProcessorFactory to return the correct resource definition
            SetupProcessorFactoryForResourceDefinition<TodoItem, TodoItem>(processorFactory, todoItemResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TodoItem>(context.Object, discovery, meta);
            return (todoItemResource, context, hookExecutor);
        }

        protected  (Mock<IJsonApiContext> context, IResourceHookExecutor<TMain>, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<IIdentifiable>>)
            CreateTestObjectsForNestedCase<TMain, TNested>(
            IImplementedResourceHooks<TMain> mainDiscovery = null,
            IImplementedResourceHooks<TNested> nestedDiscovery = null,
            List<KeyValuePair<string, ResourceHook>> orderOfExecution = null
            )
            where TMain : class, IIdentifiable
            where TNested : class, IIdentifiable
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            (var mainResource, var identifiableMainResource) = CreateResourceDefinition(mainDiscovery, orderOfExecution);
            var identifiableNestedResource = CreateResourceDefinition(nestedDiscovery, orderOfExecution).Item2;

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            SetupProcessorFactoryForResourceDefinition<TMain, TMain>(processorFactory, mainResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TMain>(context.Object, mainDiscovery, meta);

            SetupProcessorFactoryForResourceDefinition<TNested, IIdentifiable>(processorFactory, identifiableNestedResource.Object);

            return (context, hookExecutor, mainResource, identifiableNestedResource);
        }


        protected IImplementedResourceHooks<TEntity> SetDiscoverableHooks<TEntity>(ResourceHook[] implementedHooks = null)
            where TEntity : class, IIdentifiable
        {
            implementedHooks = implementedHooks ?? Enum.GetValues(typeof(ResourceHook))
                            .Cast<ResourceHook>()
                            .Where(h => h != ResourceHook.None)
                            .ToArray();
            var mock = new Mock<IImplementedResourceHooks<TEntity>>();
            mock.Setup(discovery => discovery.ImplementedHooks)
                .Returns(implementedHooks);
            return mock.Object;
        }

        private Mock<IResourceHookContainer<TImplementAs>> ImplementAs<TImplementAs, TActual>(
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

         
         private (Mock<IResourceHookContainer<TModel>>, Mock<IResourceHookContainer<IIdentifiable>>) CreateResourceDefinition
            <TModel>(IImplementedResourceHooks<TModel> discovery,
             List<KeyValuePair<string, ResourceHook>> orderOfExecution = null
            )
            where TModel : class, IIdentifiable
        {
            var resourceDefinition = new Mock<IResourceHookContainer<TModel>>();

            var identifiableResourceDefinition = ImplementAs(resourceDefinition.As<IResourceHookContainer<IIdentifiable>>(), discovery);
            var modelSpecificResourceDefinition = ImplementAs(resourceDefinition.As<IResourceHookContainer<TModel>>(), discovery);

            return (modelSpecificResourceDefinition, identifiableResourceDefinition);
        }



        private (Mock<IJsonApiContext>, Mock<IGenericProcessorFactory>) CreateContextAndProcessorMocks()
        {
            var processorFactory = new Mock<IGenericProcessorFactory>();
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);
            return (context, processorFactory);
        }


        private void SetupProcessorFactoryForResourceDefinition<TModel, TInnerContainerType>(
            Mock<IGenericProcessorFactory> processorFactory,
            IResourceHookContainer<TInnerContainerType> modelResource)
            where TModel : class, IIdentifiable
            where TInnerContainerType : class, IIdentifiable

        {
            processorFactory.Setup(c => c.GetProcessor<IResourceHookContainer>(typeof(IResourceHookContainer<>), typeof(TModel)))
            .Returns(modelResource);
        }

    }


}

