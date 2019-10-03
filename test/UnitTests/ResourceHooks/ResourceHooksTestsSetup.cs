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
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Internal.Query;

namespace UnitTests.ResourceHooks
{
    public class HooksDummyData
    {
        protected IFieldsExplorer _fieldExplorer;
        protected IResourceGraph _graph;
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
            _graph = new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .AddResource<Passport>()
                .AddResource<Article>()
                .AddResource<IdentifiableArticleTag>()
                .AddResource<Tag>()
                .AddResource<TodoItemCollection, Guid>()
                .Build();

            _fieldExplorer = new ExposedFieldExplorer(_graph);

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
        (IResourceGraph, Mock<IUpdatedFields>, Mock<IIncludedQueryService>, Mock<IGenericProcessorFactory>, IJsonApiOptions) CreateMocks()
        {
            var pfMock = new Mock<IGenericProcessorFactory>();
            var graph = _graph;
            var ufMock = new Mock<IUpdatedFields>();
            var iqsMock = new Mock<IIncludedQueryService>();
            var optionsMock = new JsonApiOptions { LoaDatabaseValues = false };
            return (graph, ufMock, iqsMock, pfMock, optionsMock);
        }

        internal (Mock<IIncludedQueryService>, ResourceHookExecutor, Mock<IResourceHookContainer<TMain>>) CreateTestObjects<TMain>(IHooksDiscovery<TMain> mainDiscovery = null)
            where TMain : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            var mainResource = CreateResourceDefinition(mainDiscovery);

            // mocking the GenericProcessorFactory and JsonApiContext and wiring them up.
            var (graph, ufMock, iqMock, gpfMock, options) = CreateMocks();

            SetupProcessorFactoryForResourceDefinition(gpfMock, mainResource.Object, mainDiscovery, null);

            var execHelper = new HookExecutorHelper(gpfMock.Object, graph, options);
            var traversalHelper = new TraversalHelper(graph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, iqMock.Object, graph);

            return (iqMock, hookExecutor, mainResource);
        }

        protected (Mock<IIncludedQueryService>, Mock<IUpdatedFields>, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TNested>>)
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
            var (graph, ufMock, iqMock, gpfMock, options) = CreateMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            SetupProcessorFactoryForResourceDefinition(gpfMock, mainResource.Object, mainDiscovery, dbContext);
            SetupProcessorFactoryForResourceDefinition(gpfMock, nestedResource.Object, nestedDiscovery, dbContext);

            var execHelper = new HookExecutorHelper(gpfMock.Object, graph, options);
            var traversalHelper = new TraversalHelper(graph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, iqMock.Object, graph);

            return (iqMock, ufMock, hookExecutor, mainResource, nestedResource);
        }

        protected (Mock<IIncludedQueryService>, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TFirstNested>>, Mock<IResourceHookContainer<TSecondNested>>)
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
            var (graph, ufMock, iqMock, gpfMock, options) = CreateMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            SetupProcessorFactoryForResourceDefinition(gpfMock, mainResource.Object, mainDiscovery, dbContext);
            SetupProcessorFactoryForResourceDefinition(gpfMock, firstNestedResource.Object, firstNestedDiscovery, dbContext);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondNestedResource.Object, secondNestedDiscovery, dbContext);

            var execHelper = new HookExecutorHelper(gpfMock.Object, graph, options);
            var traversalHelper = new TraversalHelper(graph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, iqMock.Object, graph);

            return (iqMock, hookExecutor, mainResource, firstNestedResource, secondNestedResource);
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
            .Setup(rd => rd.BeforeCreate(It.IsAny<IEntityHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((entities, context) => entities)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeRead(It.IsAny<ResourcePipeline>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeUpdate(It.IsAny<IDiffableEntityHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<DiffableEntityHashSet<TModel>, ResourcePipeline>((entities, context) => entities)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeDelete(It.IsAny<IEntityHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((entities, context) => entities)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<string>, IRelationshipsDictionary<TModel>, ResourcePipeline>((ids, context, helper) => ids)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeImplicitUpdateRelationship(It.IsAny<IRelationshipsDictionary<TModel>>(), It.IsAny<ResourcePipeline>()))
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
            context.Setup(c => c.Options).Returns(new JsonApiOptions { LoaDatabaseValues = false });
            context.Setup(c => c.ResourceGraph).Returns(ResourceGraph.Instance);

            return (context, processorFactory);
        }

        void SetupProcessorFactoryForResourceDefinition<TModel>(
        Mock<IGenericProcessorFactory> processorFactory,
        IResourceHookContainer<TModel> modelResource,
        IHooksDiscovery<TModel> discovery,
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
                var idType = TypeHelper.GetIdentifierType<TModel>();
                if (idType == typeof(int))
                {
                    IEntityReadRepository<TModel, int> repo = CreateTestRepository<TModel>(dbContext, new Mock<IJsonApiContext>().Object);
                    processorFactory.Setup(c => c.GetProcessor<IEntityReadRepository<TModel, int>>(typeof(IEntityReadRepository<,>), typeof(TModel), typeof(int))).Returns(repo);
                }
                else
                {
                    throw new TypeLoadException("Test not set up properly");
                }

            }
        }

        IEntityReadRepository<TModel, int> CreateTestRepository<TModel>(
        AppDbContext dbContext,
        IJsonApiContext apiContext
        ) where TModel : class, IIdentifiable<int>
        {
            IDbContextResolver resolver = CreateTestDbResolver<TModel>(dbContext);
            return new DefaultEntityRepository<TModel, int>(null, apiContext, resolver);
        }

        IDbContextResolver CreateTestDbResolver<TModel>(AppDbContext dbContext) where TModel : class, IIdentifiable<int>
        {
            var mock = new Mock<IDbContextResolver>();
            mock.Setup(r => r.GetContext()).Returns(dbContext);
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

        protected List<List<RelationshipAttribute>> GetIncludedRelationshipsChains(params string[] chains)
        {
            var parsedChains = new List<List<RelationshipAttribute>>();

            foreach (var chain in chains)
                parsedChains.Add(GetIncludedRelationshipsChain(chain));

            return parsedChains;
        }

        protected List<RelationshipAttribute> GetIncludedRelationshipsChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            var resourceContext = _graph.GetContextEntity<TodoItem>();
            var splittedPath = chain.Split(QueryConstants.DOT);
            foreach (var requestedRelationship in splittedPath)
            {
                var relationship = resourceContext.Relationships.Single(r => r.PublicRelationshipName == requestedRelationship);
                parsedChain.Add(relationship);
                resourceContext = _graph.GetContextEntity(relationship.DependentType);
            }
            return parsedChain;
        }
    }
}

