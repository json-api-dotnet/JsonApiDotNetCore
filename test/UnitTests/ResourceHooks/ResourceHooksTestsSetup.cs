using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Hooks.Internal.Traversal;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Person = JsonApiDotNetCoreExample.Models.Person;

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
            var appDbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().Options, new FrozenSystemClock());

            _resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<TodoItem>()
                .Add<Person>()
                .Add<Passport>()
                .Add<Article>()
                .Add<IdentifiableArticleTag>()
                .Add<Tag>()
                .Add<TodoItemCollection, Guid>()
                .Build();

            _todoFaker = new Faker<TodoItem>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
            _personFaker = new Faker<Person>().Rules((f, i) => i.Id = f.UniqueIndex + 1);

            _articleFaker = new Faker<Article>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
            _articleTagFaker = new Faker<ArticleTag>().CustomInstantiator(f => new ArticleTag());
            _identifiableArticleTagFaker = new Faker<IdentifiableArticleTag>().Rules((f, i) => i.Id = f.UniqueIndex + 1);
            _tagFaker = new Faker<Tag>()
                .CustomInstantiator(f => new Tag())
                .Rules((f, i) => i.Id = f.UniqueIndex + 1);

            _passportFaker = new Faker<Passport>()
                .CustomInstantiator(f => new Passport(appDbContext))
                .Rules((f, i) => i.Id = f.UniqueIndex + 1);
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
            articleTagsSubset.ArticleTags = joinsSubSet.ToHashSet();
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }

            var allTags = _tagFaker.Generate(3).Concat(tagsSubset).ToList();
            var completeJoin = _articleTagFaker.Generate(6);

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.ArticleTags = completeJoin.ToHashSet();

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
            articleTagsSubset.IdentifiableArticleTags = joinsSubSet.ToHashSet();
            for (int i = 0; i < 3; i++)
            {
                joinsSubSet[i].Article = articleTagsSubset;
                joinsSubSet[i].Tag = tagsSubset[i];
            }
            var allTags = _tagFaker.Generate(3).Concat(tagsSubset).ToList();
            var completeJoin = _identifiableArticleTagFaker.Generate(6);

            var articleWithAllTags = _articleFaker.Generate();
            articleWithAllTags.IdentifiableArticleTags = joinsSubSet.ToHashSet();

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
        private (Mock<ITargetedFields>, Mock<IEnumerable<IQueryConstraintProvider>>, Mock<IGenericServiceFactory>, IJsonApiOptions) CreateMocks()
        {
            var pfMock = new Mock<IGenericServiceFactory>();
            var ufMock = new Mock<ITargetedFields>();

            var constraintsMock = new Mock<IEnumerable<IQueryConstraintProvider>>();
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(new List<IQueryConstraintProvider>(new IQueryConstraintProvider[0]).GetEnumerator());

            var optionsMock = new JsonApiOptions { LoadDatabaseValues = false };
            return (ufMock, constraintsMock, pfMock, optionsMock);
        }

        internal (Mock<IEnumerable<IQueryConstraintProvider>>, ResourceHookExecutor, Mock<IResourceHookContainer<TPrimary>>) CreateTestObjects<TPrimary>(IHooksDiscovery<TPrimary> primaryDiscovery = null)
            where TPrimary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            var primaryResource = CreateResourceDefinition(primaryDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            var (ufMock, constraintsMock, gpfMock, options) = CreateMocks();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery);

            var execHelper = new HookExecutorHelper(gpfMock.Object, _resourceGraph, options);
            var traversalHelper = new TraversalHelper(_resourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, _resourceGraph);

            return (constraintsMock, hookExecutor, primaryResource);
        }

        protected (Mock<IEnumerable<IQueryConstraintProvider>>, Mock<ITargetedFields>, IResourceHookExecutor, Mock<IResourceHookContainer<TPrimary>>, Mock<IResourceHookContainer<TSecondary>>)
        CreateTestObjects<TPrimary, TSecondary>(
            IHooksDiscovery<TPrimary> primaryDiscovery = null,
            IHooksDiscovery<TSecondary> secondaryDiscovery = null,
            DbContextOptions<AppDbContext> repoDbContextOptions = null
        )
            where TPrimary : class, IIdentifiable<int>
            where TSecondary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            var primaryResource = CreateResourceDefinition(primaryDiscovery);
            var secondaryResource = CreateResourceDefinition(secondaryDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            var (ufMock, constraintsMock, gpfMock, options) = CreateMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions, new FrozenSystemClock()) : null;

            var resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<TPrimary>()
                .Add<TSecondary>()
                .Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondaryResource.Object, secondaryDiscovery, dbContext, resourceGraph);

            var execHelper = new HookExecutorHelper(gpfMock.Object, _resourceGraph, options);
            var traversalHelper = new TraversalHelper(_resourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, _resourceGraph);

            return (constraintsMock, ufMock, hookExecutor, primaryResource, secondaryResource);
        }

        protected (Mock<IEnumerable<IQueryConstraintProvider>>, IResourceHookExecutor, Mock<IResourceHookContainer<TPrimary>>, Mock<IResourceHookContainer<TFirstSecondary>>, Mock<IResourceHookContainer<TSecondSecondary>>)
        CreateTestObjects<TPrimary, TFirstSecondary, TSecondSecondary>(
            IHooksDiscovery<TPrimary> primaryDiscovery = null,
            IHooksDiscovery<TFirstSecondary> firstSecondaryDiscovery = null,
            IHooksDiscovery<TSecondSecondary> secondSecondaryDiscovery = null,
            DbContextOptions<AppDbContext> repoDbContextOptions = null
        )
        where TPrimary : class, IIdentifiable<int>
        where TFirstSecondary : class, IIdentifiable<int>
        where TSecondSecondary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            var primaryResource = CreateResourceDefinition(primaryDiscovery);
            var firstSecondaryResource = CreateResourceDefinition(firstSecondaryDiscovery);
            var secondSecondaryResource = CreateResourceDefinition(secondSecondaryDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            var (ufMock, constraintsMock, gpfMock, options) = CreateMocks();

            var dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions, new FrozenSystemClock()) : null;

            var resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance)
                .Add<TPrimary>()
                .Add<TFirstSecondary>()
                .Add<TSecondSecondary>()
                .Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, firstSecondaryResource.Object, firstSecondaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondSecondaryResource.Object, secondSecondaryDiscovery, dbContext, resourceGraph);

            var execHelper = new HookExecutorHelper(gpfMock.Object, _resourceGraph, options);
            var traversalHelper = new TraversalHelper(_resourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, _resourceGraph);

            return (constraintsMock, hookExecutor, primaryResource, firstSecondaryResource, secondSecondaryResource);
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

            using (var context = new AppDbContext(options, new FrozenSystemClock()))
            {
                seeder(context);
                ResolveInverseRelationships(context);
            }
            return options;
        }

        private void MockHooks<TModel>(Mock<IResourceHookContainer<TModel>> resourceDefinition) where TModel : class, IIdentifiable<int>
        {
            resourceDefinition
            .Setup(rd => rd.BeforeCreate(It.IsAny<IResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, context) => resources)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeRead(It.IsAny<ResourcePipeline>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeUpdate(It.IsAny<IDiffableResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<DiffableResourceHashSet<TModel>, ResourcePipeline>((resources, context) => resources)
            .Verifiable();
            resourceDefinition
            .Setup(rd => rd.BeforeDelete(It.IsAny<IResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
            .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, context) => resources)
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
            .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, context) => resources)
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
            processorFactory.Setup(c => c.Get<IResourceHookContainer>(typeof(ResourceHooksDefinition<>), typeof(TModel)))
            .Returns(modelResource);

            processorFactory.Setup(c => c.Get<IHooksDiscovery>(typeof(IHooksDiscovery<>), typeof(TModel)))
            .Returns(discovery);

            if (dbContext != null)
            {
                var idType = TypeHelper.GetIdType(typeof(TModel));
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

        private IResourceReadRepository<TModel, int> CreateTestRepository<TModel>(AppDbContext dbContext, IResourceGraph resourceGraph) 
            where TModel : class, IIdentifiable<int>
        {
            var serviceProvider = ((IInfrastructure<IServiceProvider>) dbContext).Instance;
            var resourceFactory = new ResourceFactory(serviceProvider);
            IDbContextResolver resolver = CreateTestDbResolver(dbContext);
            var targetedFields = new TargetedFields();

            return new EntityFrameworkCoreRepository<TModel, int>(targetedFields, resolver, resourceGraph, resourceFactory,
                new List<IQueryConstraintProvider>(), NullLoggerFactory.Instance);
        }

        private IDbContextResolver CreateTestDbResolver(AppDbContext dbContext)
        {
            var mock = new Mock<IDbContextResolver>();
            mock.Setup(r => r.GetContext()).Returns(dbContext);
            return mock.Object;
        }

        private void ResolveInverseRelationships(AppDbContext context)
        {
            var dbContextResolvers = new[] {new DbContextResolver<AppDbContext>(context)};
            var inverseRelationships = new InverseNavigationResolver(_resourceGraph, dbContextResolvers);
            inverseRelationships.Resolve();
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
            var splitPath = chain.Split('.');
            foreach (var requestedRelationship in splitPath)
            {
                var relationship = resourceContext.Relationships.Single(r => r.PublicName == requestedRelationship);
                parsedChain.Add(relationship);
                resourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            }
            return parsedChain;
        }

        protected IEnumerable<IQueryConstraintProvider> ConvertInclusionChains(List<List<RelationshipAttribute>> inclusionChains)
        {
            var expressionsInScope = new List<ExpressionInScope>();

            if (inclusionChains != null)
            {
                var chains = inclusionChains.Select(relationships => new ResourceFieldChainExpression(relationships)).ToList();
                var includeExpression = IncludeChainConverter.FromRelationshipChains(chains);
                expressionsInScope.Add(new ExpressionInScope(null, includeExpression));
            }

            var mock = new Mock<IQueryConstraintProvider>();
            mock.Setup(x => x.GetConstraints()).Returns(expressionsInScope);

            IQueryConstraintProvider includeConstraintProvider = mock.Object;
            return new List<IQueryConstraintProvider> {includeConstraintProvider};
        }

    }
}
