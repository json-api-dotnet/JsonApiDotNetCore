
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
        protected (Mock<IResourceHookContainer<TMain>>, Mock<IJsonApiContext>, IResourceHookExecutor<TMain>) 
        CreateTestObjects<TMain>(IImplementedResourceHooks<TMain> discovery = null)
            where TMain : class, IIdentifiable

        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            (var mainResource, var identifiableMainResource) = CreateResourceDefinition(discovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            // wiring up the mocked GenericProcessorFactory to return the correct resource definition
            SetupProcessorFactoryForResourceDefinition<TMain, TMain>(processorFactory, mainResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TMain>(context.Object, discovery, meta);

            mainResource
                .Setup(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TMain>>(), It.IsAny<ResourceAction>()))
                .Callback<IEnumerable<TMain>, ResourceAction>( (x, y) => {
                    var first = x.FirstOrDefault();
                    first.StringId = "123";
                })
                .Verifiable();
            return (mainResource, context, hookExecutor);
        }

        protected  (Mock<IJsonApiContext> context, IResourceHookExecutor<TMain>, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<IIdentifiable>>)
            CreateTestObjects<TMain, TNested>(
            IImplementedResourceHooks<TMain> mainDiscovery = null,
            IImplementedResourceHooks<TNested> nestedDiscovery = null
            )
            where TMain : class, IIdentifiable
            where TNested : class, IIdentifiable
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            (var mainResource, var identifiableMainResource) = CreateResourceDefinition(mainDiscovery);
            var identifiableNestedResource = CreateResourceDefinition(nestedDiscovery).Item2;

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            SetupProcessorFactoryForResourceDefinition<TMain, TMain>(processorFactory, mainResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TMain>(context.Object, mainDiscovery, meta);

            SetupProcessorFactoryForResourceDefinition<TNested, IIdentifiable>(processorFactory, identifiableNestedResource.Object);

            return (context, hookExecutor, mainResource, identifiableNestedResource);
        }

        protected (Mock<IJsonApiContext> context, IResourceHookExecutor<TMain>, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<IIdentifiable>>, Mock<IResourceHookContainer<IIdentifiable>>)
            CreateTestObjects<TMain, TFirstNested, TSecondNested>(
            IImplementedResourceHooks<TMain> mainDiscovery = null,
            IImplementedResourceHooks<TFirstNested> firstNestedDiscovery = null,
            IImplementedResourceHooks<TSecondNested> secondNestedDiscovery = null
            )
            where TMain : class, IIdentifiable
            where TFirstNested : class, IIdentifiable
            where TSecondNested : class, IIdentifiable
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            (var mainResource, var identifiableMainResource) = CreateResourceDefinition(mainDiscovery);
            var identifiableFirstNestedResource = CreateResourceDefinition(firstNestedDiscovery).Item2;
            var identifiableSecondNestedResource = CreateResourceDefinition(secondNestedDiscovery).Item2;

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            SetupProcessorFactoryForResourceDefinition<TMain, TMain>(processorFactory, mainResource.Object);
            var meta = new ResourceHookMetaInfo(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor<TMain>(context.Object, mainDiscovery, meta);

            SetupProcessorFactoryForResourceDefinition<TFirstNested, IIdentifiable>(processorFactory, identifiableFirstNestedResource.Object);
            SetupProcessorFactoryForResourceDefinition<TFirstNested, IIdentifiable>(processorFactory, identifiableSecondNestedResource.Object);

            return (context, hookExecutor, mainResource, identifiableFirstNestedResource, identifiableSecondNestedResource);
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
                .Returns<ResourceHook>((hook) => discovery.ImplementedHooks.Contains(hook))
                .Verifiable();
            resourceDefinition
                .As<IResourceHookContainer<TActual>>()
                .Setup(rd => rd.ShouldExecuteHook(It.IsAny<ResourceHook>()))
                .Returns<ResourceHook>((hook) => discovery.ImplementedHooks.Contains(hook))
                .Verifiable();
            return resourceDefinition;
        }

         
         private (Mock<IResourceHookContainer<TModel>>, Mock<IResourceHookContainer<IIdentifiable>>) CreateResourceDefinition
            <TModel>(IImplementedResourceHooks<TModel> discovery
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

