
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


        protected List<TodoItem> CreateTodoWithOwner()
        {
            var todoItem = new TodoItem();
            var todoList = new List<TodoItem>() { todoItem };
            var person = new Person() { AssignedTodoItems = todoList };
            todoItem.Owner = person;
            return todoList;
        }
        protected (Mock<IJsonApiContext>, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>) 
        CreateTestObjects<TMain>(IHooksDiscovery<TMain> discovery = null)
            where TMain : class, IIdentifiable

        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            var mainResource = CreateResourceDefinition(discovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            // wiring up the mocked GenericProcessorFactory to return the correct resource definition
            SetupProcessorFactoryForResourceDefinition(processorFactory, mainResource.Object, discovery);
            var meta = new HookExecutorHelper(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor(meta);

            return (context, hookExecutor, mainResource);
        }

        protected  (Mock<IJsonApiContext> context, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TNested>>)
            CreateTestObjects<TMain, TNested>(
            IHooksDiscovery<TMain> mainDiscovery = null,
            IHooksDiscovery<TNested> nestedDiscovery = null,
            Action<Mock<IJsonApiContext>> optionalMockAction = null 
            )
            where TMain : class, IIdentifiable
            where TNested : class, IIdentifiable
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            var mainResource = CreateResourceDefinition(mainDiscovery);
            var nestedResource = CreateResourceDefinition(nestedDiscovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            optionalMockAction?.Invoke(context);


            SetupProcessorFactoryForResourceDefinition(processorFactory, mainResource.Object, mainDiscovery);
            var meta = new HookExecutorHelper(context.Object.GenericProcessorFactory, ResourceGraph.Instance, context.Object);
            var hookExecutor = new ResourceHookExecutor(meta);

            SetupProcessorFactoryForResourceDefinition(processorFactory, nestedResource.Object, nestedDiscovery);

            return (context, hookExecutor, mainResource, nestedResource);
        }

        protected (Mock<IJsonApiContext> context, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TFirstNested>>, Mock<IResourceHookContainer<TSecondNested>>)
            CreateTestObjects<TMain, TFirstNested, TSecondNested>(
            IHooksDiscovery<TMain> mainDiscovery = null,
            IHooksDiscovery<TFirstNested> firstNestedDiscovery = null,
            IHooksDiscovery<TSecondNested> secondNestedDiscovery = null
            )
            where TMain : class, IIdentifiable
            where TFirstNested : class, IIdentifiable
            where TSecondNested : class, IIdentifiable
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            var mainResource = CreateResourceDefinition(mainDiscovery);
            var firstNestedResource = CreateResourceDefinition(firstNestedDiscovery);
            var secondNestedResource = CreateResourceDefinition(secondNestedDiscovery);


            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            SetupProcessorFactoryForResourceDefinition(processorFactory, mainResource.Object, mainDiscovery);
            var meta = new HookExecutorHelper(context.Object.GenericProcessorFactory, ResourceGraph.Instance);
            var hookExecutor = new ResourceHookExecutor(meta);

            SetupProcessorFactoryForResourceDefinition(processorFactory, firstNestedResource.Object, firstNestedDiscovery);
            SetupProcessorFactoryForResourceDefinition(processorFactory, secondNestedResource.Object, secondNestedDiscovery);

            return (context, hookExecutor, mainResource, firstNestedResource, secondNestedResource);
        }

        protected IHooksDiscovery<TEntity> SetDiscoverableHooks<TEntity>(ResourceHook[] implementedHooks = null)
            where TEntity : class, IIdentifiable
        {
            implementedHooks = implementedHooks ?? Enum.GetValues(typeof(ResourceHook))
                            .Cast<ResourceHook>()
                            .Where(h => h != ResourceHook.None)
                            .ToArray();
            var mock = new Mock<IHooksDiscovery<TEntity>>();
            mock.Setup(discovery => discovery.ImplementedHooks)
                .Returns(implementedHooks);
            return mock.Object;
        }

       private Mock<IResourceHookContainer<TModel>> CreateResourceDefinition
           <TModel>(IHooksDiscovery<TModel> discovery
           )
           where TModel : class, IIdentifiable
        {
            var resourceDefinition = new Mock<IResourceHookContainer<TModel>>();
            MockHooks(resourceDefinition, discovery);
            return resourceDefinition;
        }

        private void MockHooks<TModel>(
            Mock<IResourceHookContainer<TModel>> resourceDefinition,
            IHooksDiscovery<TModel> discovery
            ) where TModel : class, IIdentifiable
        {
            resourceDefinition
                .Setup(rd => rd.BeforeCreate(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TModel>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterCreate(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TModel>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), null))
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterRead(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TModel>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeUpdate(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TModel>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterUpdate(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>()))
                .Returns<IEnumerable<TModel>, ResourceAction>((entities, action) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>()))
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterDelete(It.IsAny<IEnumerable<TModel>>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()))
                .Verifiable();
        }

        private (Mock<IJsonApiContext>, Mock<IGenericProcessorFactory>) CreateContextAndProcessorMocks()
        {
            var processorFactory = new Mock<IGenericProcessorFactory>();
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);
            return (context, processorFactory);
        }

        private void SetupProcessorFactoryForResourceDefinition<TModel>(
            Mock<IGenericProcessorFactory> processorFactory,
            IResourceHookContainer<TModel> modelResource,
            IHooksDiscovery<TModel> discovery)
            where TModel : class, IIdentifiable

        {
            processorFactory.Setup(c => c.GetProcessor<IResourceHookContainer>(typeof(ResourceDefinition<>), typeof(TModel)))
            .Returns(modelResource);

            processorFactory.Setup(c => c.GetProcessor<IHooksDiscovery>(typeof(IHooksDiscovery<>), typeof(TModel)))
            .Returns(discovery);
        }
    }
}

