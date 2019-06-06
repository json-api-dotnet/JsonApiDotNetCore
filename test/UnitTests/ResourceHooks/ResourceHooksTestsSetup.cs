using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Person = JsonApiDotNetCoreExample.Models.Person;

namespace UnitTests.ResourceHooks
{
    public class HooksDummyData
    {
        protected ResourceHook[] NoHooks = new ResourceHook[0];
        protected ResourceHook[] EnableDbValues = { ResourceHook.BeforeUpdate, ResourceHook.BeforeUpdateRelationship };
        protected ResourceHook[] DisableDbValues = new ResourceHook[0];
        protected readonly Faker<Person> _personFaker;
        protected readonly Faker<TodoItem> _todoFaker;
        protected readonly Faker<Tag> _tagFaker;
        protected readonly Faker<Article> _articleFaker;
        protected readonly Faker<ArticleTag> _articleTagFaker;
        protected readonly Faker<IdentifiableArticleTag> _identifiableArticleTagFaker;
        protected readonly Faker<Passport> _passportFaker;
        public HooksDummyData()
        {
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .AddResource<Passport>()
                .AddResource<Article>()
                .AddResource<IdentifiableArticleTag>()
                .AddResource<Tag>()
                .Build();

            _todoFaker = new Faker<TodoItem>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
            _personFaker = new Faker<Person>().Rules((f, i) => i.Id = f.UniqueIndex + 1);

            _articleFaker = new Faker<Article>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
            _articleTagFaker = new Faker<ArticleTag>();
            _identifiableArticleTagFaker = new Faker<IdentifiableArticleTag>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
            _tagFaker = new Faker<Tag>().Rules((f, i) => i.Id = f.UniqueIndex + 1);

            _passportFaker = new Faker<Passport>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
        }

        protected List<TodoItem> CreateTodoWithToOnePerson()
        {
            var todoItem = _todoFaker.Generate();
            var person = _personFaker.Generate();
            var todoList = new List<TodoItem>() { todoItem };
            person.ToOneTodoItem = todoItem;
            todoItem.ToOnePerson = person;
            return todoList;
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

        protected (List<Article>, List<ArticleTag>, List<Tag>) CreateManyToManyData()
        {
            var tagsSubset = _tagFaker.Generate(3).ToList();
            var joinsSubSet = _articleTagFaker.Generate(3).ToList();
            var articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet;
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            var allTags = _tagFaker.Generate(3).ToList().Concat(tagsSubset).ToList();
            var completeJoin = _articleTagFaker.Generate(6).ToList();

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin;

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            var allJoins = joinsSubSet.Concat(completeJoin).ToList();

            var articles = new List<Article>() { articleTagsSubset, articleWithAllTags };
            return (articles, allJoins, allTags);
        }

        protected (List<Article>, List<IdentifiableArticleTag>, List<Tag>) CreateIdentifiableManyToManyData()
        {
            var tagsSubset = _tagFaker.Generate(3).ToList();
            var joinsSubSet = _identifiableArticleTagFaker.Generate(3).ToList();
            var articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.IdentifiableArticleTags = joinsSubSet;
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }
            var allTags = _tagFaker.Generate(3).ToList().Concat(tagsSubset).ToList();
            var completeJoin = _identifiableArticleTagFaker.Generate(6).ToList();

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.IdentifiableArticleTags = joinsSubSet;

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            var allJoins = joinsSubSet.Concat(completeJoin).ToList();
            var articles = new List<Article>() { articleTagsSubset, articleWithAllTags };
            return (articles, allJoins, allTags);
        }

    }

    public class HooksTestsSetup : HooksDummyData
    {
        protected (Mock<IJsonApiContext>, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>)
        CreateTestObjects<TMain>(IHooksDiscovery<TMain> discovery = null)
        where TMain : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            var mainResource = CreateResourceDefinition(discovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            // wiring up the mocked GenericProcessorFactory to return the correct resource definition
            SetupProcessorFactoryForResourceDefinition(processorFactory, mainResource.Object, discovery, context.Object);
            var meta = new HookExecutorHelper(context.Object.GenericProcessorFactory, ResourceGraph.Instance, context.Object);
            var hookExecutor = new ResourceHookExecutor(meta, context.Object, ResourceGraph.Instance);

            return (context, hookExecutor, mainResource);
        }

        protected (Mock<IJsonApiContext> context, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TNested>>)
        CreateTestObjects<TMain, TNested>(
        IHooksDiscovery<TMain> mainDiscovery = null,
        IHooksDiscovery<TNested> nestedDiscovery = null,
        DbContextOptions<AppDbContext> repoDbContextOptions = null
        )
        where TMain : class, IIdentifiable<int>
        where TNested : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            var mainResource = CreateResourceDefinition(mainDiscovery);
            var nestedResource = CreateResourceDefinition(nestedDiscovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();


            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            SetupProcessorFactoryForResourceDefinition(processorFactory, mainResource.Object, mainDiscovery, context.Object, dbContext);
            var meta = new HookExecutorHelper(context.Object.GenericProcessorFactory, ResourceGraph.Instance, context.Object);
            var hookExecutor = new ResourceHookExecutor(meta, context.Object, ResourceGraph.Instance);

            SetupProcessorFactoryForResourceDefinition(processorFactory, nestedResource.Object, nestedDiscovery, context.Object, dbContext);

            return (context, hookExecutor, mainResource, nestedResource);
        }

        protected (Mock<IJsonApiContext> context, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TFirstNested>>, Mock<IResourceHookContainer<TSecondNested>>)
        CreateTestObjects<TMain, TFirstNested, TSecondNested>(
        IHooksDiscovery<TMain> mainDiscovery = null,
        IHooksDiscovery<TFirstNested> firstNestedDiscovery = null,
        IHooksDiscovery<TSecondNested> secondNestedDiscovery = null,
        DbContextOptions<AppDbContext> repoDbContextOptions = null
        )
        where TMain : class, IIdentifiable<int>
        where TFirstNested : class, IIdentifiable<int>
        where TSecondNested : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            var mainResource = CreateResourceDefinition(mainDiscovery);
            var firstNestedResource = CreateResourceDefinition(firstNestedDiscovery);
            var secondNestedResource = CreateResourceDefinition(secondNestedDiscovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            (var context, var processorFactory) = CreateContextAndProcessorMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            SetupProcessorFactoryForResourceDefinition(processorFactory, mainResource.Object, mainDiscovery, context.Object, dbContext);
            var meta = new HookExecutorHelper(context.Object.GenericProcessorFactory, ResourceGraph.Instance, context.Object);
            var hookExecutor = new ResourceHookExecutor(meta, context.Object, ResourceGraph.Instance);

            SetupProcessorFactoryForResourceDefinition(processorFactory, firstNestedResource.Object, firstNestedDiscovery, context.Object, dbContext);
            SetupProcessorFactoryForResourceDefinition(processorFactory, secondNestedResource.Object, secondNestedDiscovery, context.Object, dbContext);

            return (context, hookExecutor, mainResource, firstNestedResource, secondNestedResource);
        }

        protected IHooksDiscovery<TEntity> SetDiscoverableHooks<TEntity>(ResourceHook[] implementedHooks, params ResourceHook[] enableDbValuesHooks)
        where TEntity : class, IIdentifiable<int>
        {
            var mock = new Mock<IHooksDiscovery<TEntity>>();
            mock.Setup(discovery => discovery.ImplementedHooks)
            .Returns(implementedHooks);

            if (!enableDbValuesHooks.Any())
            {
                mock.Setup(discovery => discovery.DatabaseValuesDisabledHooks)
                .Returns(enableDbValuesHooks);
            }
            mock.Setup(discovery => discovery.DatabaseValuesEnabledHooks)
            .Returns(new ResourceHook[] { ResourceHook.BeforeImplicitUpdateRelationship }.Concat(enableDbValuesHooks).ToArray());

            return mock.Object;
        }

        protected void VerifyNoOtherCalls(params dynamic[] resourceMocks)
        {
            foreach (var mock in resourceMocks)
            {
                mock.VerifyNoOtherCalls();
            }
        }

        protected DbContextOptions<AppDbContext> InitInMemoryDb(Action<DbContext> seeder)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "repository_mock")
            .Options;

            using (var context = new AppDbContext(options))
            {
                seeder(context);
                ResolveInverseRelationships(context);
            }
            return options;
        }

        void MockHooks<TModel>(Mock<IResourceHookContainer<TModel>> resourceDefinition) where TModel : class, IIdentifiable<int>
        {
            resourceDefinition
            .Setup(rd => rd.BeforeCreate(It.IsAny<IAffectedResources<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((entities, context) => entities)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeRead(It.IsAny<ResourcePipeline>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeUpdate(It.IsAny<IEntityDiff<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<EntityDiff<TModel>, ResourcePipeline>((entityDiff, context) => entityDiff.Entities)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeDelete(It.IsAny<IAffectedResources<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((entities, context) => entities)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IAffectedRelationships<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<string>, IAffectedRelationships<TModel>, ResourcePipeline>((ids, context, helper) => ids)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeImplicitUpdateRelationship(It.IsAny<IAffectedRelationships<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Verifiable();

            resourceDefinition
            .Setup(rd => rd.OnReturn(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((entities, context) => entities)
            .Verifiable();

            resourceDefinition
            .Setup(rd => rd.AfterCreate(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.AfterRead(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>(), It.IsAny<bool>()))
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.AfterUpdate(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.AfterDelete(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>(), It.IsAny<bool>()))
            .Verifiable();


        }

        (Mock<IJsonApiContext>, Mock<IGenericProcessorFactory>) CreateContextAndProcessorMocks()
        {
            var processorFactory = new Mock<IGenericProcessorFactory>();
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);
            context.Setup(c => c.Options).Returns(new JsonApiOptions { LoadDatabaseValues = false });
            context.Setup(c => c.ResourceGraph).Returns(ResourceGraph.Instance);

            return (context, processorFactory);
        }

        void SetupProcessorFactoryForResourceDefinition<TModel>(
        Mock<IGenericProcessorFactory> processorFactory,
        IResourceHookContainer<TModel> modelResource,
        IHooksDiscovery<TModel> discovery,
        IJsonApiContext apiContext,
        AppDbContext dbContext = null
        )
        where TModel : class, IIdentifiable<int>
        {
            processorFactory.Setup(c => c.GetProcessor<IResourceHookContainer>(typeof(ResourceDefinition<>), typeof(TModel)))
            .Returns(modelResource);

            processorFactory.Setup(c => c.GetProcessor<IHooksDiscovery>(typeof(IHooksDiscovery<>), typeof(TModel)))
            .Returns(discovery);

            if (dbContext != null)
            {
                IEntityReadRepository<TModel, int> repo = CreateTestRepository<TModel>(dbContext, apiContext);
                processorFactory.Setup(c => c.GetProcessor<IEntityReadRepository<TModel, int>>(typeof(IEntityReadRepository<,>), typeof(TModel), typeof(int))).Returns(repo);
            }
        }

        IEntityReadRepository<TModel, int> CreateTestRepository<TModel>(
        AppDbContext dbContext,
        IJsonApiContext apiContext
        ) where TModel : class, IIdentifiable<int>
        {
            IDbContextResolver resolver = CreateTestDbResolver<TModel>(dbContext);
            return new DefaultEntityRepository<TModel, int>(apiContext, resolver);
        }

        IDbContextResolver CreateTestDbResolver<TModel>(AppDbContext dbContext) where TModel : class, IIdentifiable<int>
        {
            var mock = new Mock<IDbContextResolver>();
            mock.Setup(r => r.GetContext()).Returns(dbContext);
            mock.Setup(r => r.GetDbSet<TModel>()).Returns(dbContext.Set<TModel>());
            return mock.Object;
        }

        void ResolveInverseRelationships(AppDbContext context)
        {
            new InverseRelationships(ResourceGraph.Instance, new DbContextResolver<AppDbContext>(context)).Resolve();
        }

        Mock<IResourceHookContainer<TModel>> CreateResourceDefinition
        <TModel>(IHooksDiscovery<TModel> discovery
        )
        where TModel : class, IIdentifiable<int>
        {
            var resourceDefinition = new Mock<IResourceHookContainer<TModel>>();
            MockHooks(resourceDefinition);
            return resourceDefinition;
        }
    }
}

