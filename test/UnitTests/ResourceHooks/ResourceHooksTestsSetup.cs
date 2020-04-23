using Bogus;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
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
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.ResourceHooks
{
    public class HooksDummyData
    {
        protected IResourceGraph _resourceGraph;
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
            _resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .AddResource<Passport>()
                .AddResource<Article>()
                .AddResource<IdentifiableArticleTag>()
                .AddResource<Tag>()
                .AddResource<TodoItemCollection, Guid>()
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
            var todoList = new List<TodoItem> { todoItem };
            person.OneToOneTodoItem = todoItem;
            todoItem.OneToOnePerson = person;
            return todoList;
        }

        protected HashSet<TodoItem> CreateTodoWithOwner()
        {
            var todoItem = _todoFaker.Generate();
            var person = _personFaker.Generate();
            var todoList = new HashSet<TodoItem> { todoItem };
            person.AssignedTodoItems = todoList;
            todoItem.Owner = person;
            return todoList;
        }

        protected (List<Article>, List<ArticleTag>, List<Tag>) CreateManyToManyData()
        {
            var tagsSubset = _tagFaker.Generate(3);
            var joinsSubSet = _articleTagFaker.Generate(3);
            var articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.ArticleTags = joinsSubSet;
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            var allTags = _tagFaker.Generate(3).Concat(tagsSubset).ToList();
            var completeJoin = _articleTagFaker.Generate(6);

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin;

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            var allJoins = joinsSubSet.Concat(completeJoin).ToList();

            var articles = new List<Article> { articleTagsSubset, articleWithAllTags };
            return (articles, allJoins, allTags);
        }

        protected (List<Article>, List<IdentifiableArticleTag>, List<Tag>) CreateIdentifiableManyToManyData()
        {
            var tagsSubset = _tagFaker.Generate(3);
            var joinsSubSet = _identifiableArticleTagFaker.Generate(3);
            var articleTagsSubset = _articleFaker.Generate();
            articleTagsSubset.IdentifiableArticleTags = joinsSubSet;
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }
            var allTags = _tagFaker.Generate(3).Concat(tagsSubset).ToList();
            var completeJoin = _identifiableArticleTagFaker.Generate(6);

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.IdentifiableArticleTags = joinsSubSet;

            for (int i = 0; i < 6; i++)
            {
                completeJoin[i].Article = articleWithAllTags;
                completeJoin[i].Tag = allTags[i];
            }

            var allJoins = joinsSubSet.Concat(completeJoin).ToList();
            var articles = new List<Article> { articleTagsSubset, articleWithAllTags };
            return (articles, allJoins, allTags);
        }
    }

    public class HooksTestsSetup : HooksDummyData
    {
        private (Mock<ITargetedFields>, Mock<IIncludeService>, Mock<IGenericServiceFactory>, IJsonApiOptions) CreateMocks()
        {
            var pfMock = new Mock<IGenericServiceFactory>();
            var ufMock = new Mock<ITargetedFields>();
            var iqsMock = new Mock<IIncludeService>();
            var optionsMock = new JsonApiOptions { LoadDatabaseValues = false };
            return (ufMock, iqsMock, pfMock, optionsMock);
        }

        internal (Mock<IIncludeService>, ResourceHookExecutor, Mock<IResourceHookContainer<TMain>>) CreateTestObjects<TMain>(IHooksDiscovery<TMain> mainDiscovery = null)
            where TMain : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            var mainResource = CreateResourceDefinition(mainDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            var (ufMock, iqMock, gpfMock, options) = CreateMocks();

            SetupProcessorFactoryForResourceDefinition(gpfMock, mainResource.Object, mainDiscovery);

            var execHelper = new HookExecutorHelper(gpfMock.Object, options);
            var traversalHelper = new TraversalHelper(_resourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, iqMock.Object, _resourceGraph);

            return (iqMock, hookExecutor, mainResource);
        }

        protected (Mock<IIncludeService>, Mock<ITargetedFields>, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TNested>>)
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

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            var (ufMock, iqMock, gpfMock, options) = CreateMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            var resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .AddResource<TMain>()
                .AddResource<TNested>()
                .Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, mainResource.Object, mainDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, nestedResource.Object, nestedDiscovery, dbContext, resourceGraph);

            var execHelper = new HookExecutorHelper(gpfMock.Object, options);
            var traversalHelper = new TraversalHelper(_resourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, iqMock.Object, _resourceGraph);

            return (iqMock, ufMock, hookExecutor, mainResource, nestedResource);
        }

        protected (Mock<IIncludeService>, IResourceHookExecutor, Mock<IResourceHookContainer<TMain>>, Mock<IResourceHookContainer<TFirstNested>>, Mock<IResourceHookContainer<TSecondNested>>)
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

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            var (ufMock, iqMock, gpfMock, options) = CreateMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            var resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .AddResource<TMain>()
                .AddResource<TFirstNested>()
                .AddResource<TSecondNested>()
                .Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, mainResource.Object, mainDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, firstNestedResource.Object, firstNestedDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondNestedResource.Object, secondNestedDiscovery, dbContext, resourceGraph);

            var execHelper = new HookExecutorHelper(gpfMock.Object, options);
            var traversalHelper = new TraversalHelper(_resourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, iqMock.Object, _resourceGraph);

            return (iqMock, hookExecutor, mainResource, firstNestedResource, secondNestedResource);
        }

        protected IHooksDiscovery<TResource> SetDiscoverableHooks<TResource>(ResourceHook[] implementedHooks, params ResourceHook[] enableDbValuesHooks)
        where TResource : class, IIdentifiable<int>
        {
            var mock = new Mock<IHooksDiscovery<TResource>>();
            mock.Setup(discovery => discovery.ImplementedHooks)
            .Returns(implementedHooks);

            if (!enableDbValuesHooks.Any())
            {
                mock.Setup(discovery => discovery.DatabaseValuesDisabledHooks)
                .Returns(enableDbValuesHooks);
            }
            mock.Setup(discovery => discovery.DatabaseValuesEnabledHooks)
            .Returns(new[] { ResourceHook.BeforeImplicitUpdateRelationship }.Concat(enableDbValuesHooks).ToArray());

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

        private void MockHooks<TModel>(Mock<IResourceHookContainer<TModel>> resourceDefinition) where TModel : class, IIdentifiable<int>
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

        private void SetupProcessorFactoryForResourceDefinition<TModel>(
        Mock<IGenericServiceFactory> processorFactory,
        IResourceHookContainer<TModel> modelResource,
        IHooksDiscovery<TModel> discovery,
        AppDbContext dbContext = null,
        IResourceGraph resourceGraph = null
        )
        where TModel : class, IIdentifiable<int>
        {
            processorFactory.Setup(c => c.Get<IResourceHookContainer>(typeof(ResourceDefinition<>), typeof(TModel)))
            .Returns(modelResource);

            processorFactory.Setup(c => c.Get<IHooksDiscovery>(typeof(IHooksDiscovery<>), typeof(TModel)))
            .Returns(discovery);

            if (dbContext != null)
            {
                var idType = TypeHelper.GetIdentifierType<TModel>();
                if (idType == typeof(int))
                {
                    IResourceReadRepository<TModel, int> repo = CreateTestRepository<TModel>(dbContext, resourceGraph);
                    processorFactory.Setup(c => c.Get<IResourceReadRepository<TModel, int>>(typeof(IResourceReadRepository<,>), typeof(TModel), typeof(int))).Returns(repo);
                }
                else
                {
                    throw new TypeLoadException("Test not set up properly");
                }

            }
        }

        private IResourceReadRepository<TModel, int> CreateTestRepository<TModel>(
        AppDbContext dbContext, IResourceGraph resourceGraph
        ) where TModel : class, IIdentifiable<int>
        {
            IDbContextResolver resolver = CreateTestDbResolver<TModel>(dbContext);
            return new DefaultResourceRepository<TModel, int>(null, resolver, resourceGraph, null, NullLoggerFactory.Instance);
        }

        private IDbContextResolver CreateTestDbResolver<TModel>(AppDbContext dbContext) where TModel : class, IIdentifiable<int>
        {
            var mock = new Mock<IDbContextResolver>();
            mock.Setup(r => r.GetContext()).Returns(dbContext);
            return mock.Object;
        }

        private void ResolveInverseRelationships(AppDbContext context)
        {
            new InverseRelationships(_resourceGraph, new DbContextResolver<AppDbContext>(context)).Resolve();
        }

        private Mock<IResourceHookContainer<TModel>> CreateResourceDefinition
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
            var resourceContext = _resourceGraph.GetResourceContext<TodoItem>();
            var splitPath = chain.Split(QueryConstants.DOT);
            foreach (var requestedRelationship in splitPath)
            {
                var relationship = resourceContext.Relationships.Single(r => r.PublicRelationshipName == requestedRelationship);
                parsedChain.Add(relationship);
                resourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            }
            return parsedChain;
        }
    }
}
