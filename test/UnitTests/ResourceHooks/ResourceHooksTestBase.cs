
using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
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
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace UnitTests.ResourceHooks
{

    public class ResourceHooksTestBase
    {
        protected readonly Faker<Person> _personFaker;
        protected readonly Faker<TodoItem> _todoFaker;
        protected readonly Faker<Tag> _tagFaker;
        protected readonly Faker<Article> _articleFaker;
        protected readonly Faker<ArticleTag> _articleTagFaker;
        protected readonly Faker<IdentifiableArticleTag> _identifiableArticleTagFaker;
        public ResourceHooksTestBase()
        {
            _todoFaker = new Faker<TodoItem>().Rules((f, i) => i.StringId = f.UniqueIndex.ToString());
            _personFaker = new Faker<Person>().Rules((f, i) => i.StringId = f.UniqueIndex.ToString());

            _articleFaker = new Faker<Article>().Rules((f, i) => i.StringId = f.UniqueIndex.ToString());
            _articleTagFaker = new Faker<ArticleTag>();
            _identifiableArticleTagFaker = new Faker<IdentifiableArticleTag>().Rules((f, i) => i.StringId = f.UniqueIndex.ToString());
            _tagFaker = new Faker<Tag>().Rules((f, i) => i.StringId = f.UniqueIndex.ToString());
        }

        protected List<TodoItem> CreateTodoWithOwner()
        {
            var todoItem = _todoFaker.Generate();
            var person = _personFaker.Generate();
            var todoList = new List<TodoItem>() { todoItem };
            person.AssignedTodoItems = todoList;
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
            var hookExecutor = new ResourceHookExecutor(meta, context.Object, ResourceGraph.Instance);

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
            var hookExecutor = new ResourceHookExecutor(meta, context.Object, ResourceGraph.Instance);

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
            var hookExecutor = new ResourceHookExecutor(meta, context.Object, ResourceGraph.Instance);

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
               .Setup(rd => rd.BeforeCreate(It.IsAny<EntityDiff<TModel>>(), It.IsAny<HookExecutionContext<TModel>>()))
               .Returns<EntityDiff<TModel>, HookExecutionContext<TModel>>((entityDiff, context) => entityDiff.RequestEntities)
               .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterCreate(It.IsAny<IEnumerable<TModel>>(), It.IsAny<HookExecutionContext<TModel>>()))
                .Returns<IEnumerable<TModel>, HookExecutionContext<TModel>>((entities, context) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterRead(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>(), It.IsAny<bool>()))
                .Returns<IEnumerable<TModel>, ResourceAction, bool>((entities, context, nested) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<TModel>>(), It.IsAny<ResourceAction>()))
                .Returns<EntityDiff<TModel>, ResourceAction>((entityDiff, context) => entityDiff.RequestEntities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeUpdateRelation(It.IsAny<IEnumerable<TModel>>(), It.IsAny<ResourceAction>(), It.IsAny<IUpdatedRelationshipHelper<TModel>>()))
                .Returns<IEnumerable<TModel>, ResourceAction, IUpdatedRelationshipHelper<TModel>>((entities, context, helper) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterUpdate(It.IsAny<IEnumerable<TModel>>(), It.IsAny<HookExecutionContext<TModel>>()))
                .Returns<IEnumerable<TModel>, HookExecutionContext<TModel>>((entities, context) => entities)
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.BeforeDelete(It.IsAny<IEnumerable<TModel>>(), It.IsAny<HookExecutionContext<TModel>>()))
                .Verifiable();
            resourceDefinition
                .Setup(rd => rd.AfterDelete(It.IsAny<IEnumerable<TModel>>(), It.IsAny<HookExecutionContext<TModel>>(), It.IsAny<bool>()))
                .Verifiable();
        }


        private (Mock<IJsonApiContext>, Mock<IGenericProcessorFactory>) CreateContextAndProcessorMocks()
        {
            var processorFactory = new Mock<IGenericProcessorFactory>();
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);
            context.Setup(c => c.Options).Returns( new JsonApiOptions { DatabaseValuesInDiffs = false });
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

        protected void VerifyNoOtherCalls(params dynamic[] resourceMocks)
        {
            foreach (var mock in resourceMocks)
            {
                mock.VerifyNoOtherCalls();
            }
        }

    }
}

