using System;
using System.Collections.Generic;
using System.Linq;
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

namespace UnitTests.ResourceHooks
{
    public class HooksTestsSetup : HooksDummyData
    {
        private static readonly IncludeChainConverter IncludeChainConverter = new IncludeChainConverter();
        private static readonly HooksObjectFactory ObjectFactory = new HooksObjectFactory();

        private TestMocks CreateMocks()
        {
            var genericServiceFactoryMock = new Mock<IGenericServiceFactory>();
            var targetedFieldsMock = new Mock<ITargetedFields>();

            var constraintsMock = new Mock<IEnumerable<IQueryConstraintProvider>>();
            constraintsMock.Setup(providers => providers.GetEnumerator()).Returns(Enumerable.Empty<IQueryConstraintProvider>().GetEnumerator());

            var optionsMock = new JsonApiOptions
            {
                LoadDatabaseValues = false
            };

            return new TestMocks(targetedFieldsMock, constraintsMock, genericServiceFactoryMock, optionsMock);
        }

        protected TestObjectsA<TPrimary> CreateTestObjects<TPrimary>(IHooksDiscovery<TPrimary> primaryDiscovery = null)
            where TPrimary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            Mock<IResourceHookContainer<TPrimary>> primaryResource = CreateResourceDefinition<TPrimary>();

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            (Mock<ITargetedFields> ufMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<IGenericServiceFactory> gpfMock,
                IJsonApiOptions options) = CreateMocks();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery);

            var execHelper = new HookContainerProvider(gpfMock.Object, ResourceGraph, options);
            var traversalHelper = new NodeNavigator(ResourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, ResourceGraph);

            return new TestObjectsA<TPrimary>(hookExecutor, primaryResource);
        }

        protected TestObjectsB<TPrimary, TSecondary> CreateTestObjects<TPrimary, TSecondary>(IHooksDiscovery<TPrimary> primaryDiscovery = null,
            IHooksDiscovery<TSecondary> secondaryDiscovery = null, DbContextOptions<AppDbContext> repoDbContextOptions = null)
            where TPrimary : class, IIdentifiable<int>
            where TSecondary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            Mock<IResourceHookContainer<TPrimary>> primaryResource = CreateResourceDefinition<TPrimary>();
            Mock<IResourceHookContainer<TSecondary>> secondaryResource = CreateResourceDefinition<TSecondary>();

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            (Mock<ITargetedFields> ufMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<IGenericServiceFactory> gpfMock,
                IJsonApiOptions options) = CreateMocks();

            AppDbContext dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TPrimary>().Add<TSecondary>().Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondaryResource.Object, secondaryDiscovery, dbContext, resourceGraph);

            var execHelper = new HookContainerProvider(gpfMock.Object, ResourceGraph, options);
            var traversalHelper = new NodeNavigator(ResourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, ResourceGraph);

            return new TestObjectsB<TPrimary, TSecondary>(constraintsMock, ufMock, hookExecutor, primaryResource, secondaryResource);
        }

        protected TestObjectsC<TPrimary, TFirstSecondary, TSecondSecondary> CreateTestObjectsC<TPrimary, TFirstSecondary, TSecondSecondary>(
            IHooksDiscovery<TPrimary> primaryDiscovery = null, IHooksDiscovery<TFirstSecondary> firstSecondaryDiscovery = null,
            IHooksDiscovery<TSecondSecondary> secondSecondaryDiscovery = null, DbContextOptions<AppDbContext> repoDbContextOptions = null)
            where TPrimary : class, IIdentifiable<int>
            where TFirstSecondary : class, IIdentifiable<int>
            where TSecondSecondary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            Mock<IResourceHookContainer<TPrimary>> primaryResource = CreateResourceDefinition<TPrimary>();
            Mock<IResourceHookContainer<TFirstSecondary>> firstSecondaryResource = CreateResourceDefinition<TFirstSecondary>();
            Mock<IResourceHookContainer<TSecondSecondary>> secondSecondaryResource = CreateResourceDefinition<TSecondSecondary>();

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            (Mock<ITargetedFields> ufMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<IGenericServiceFactory> gpfMock,
                IJsonApiOptions options) = CreateMocks();

            AppDbContext dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TPrimary>().Add<TFirstSecondary>()
                .Add<TSecondSecondary>().Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, firstSecondaryResource.Object, firstSecondaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondSecondaryResource.Object, secondSecondaryDiscovery, dbContext, resourceGraph);

            var execHelper = new HookContainerProvider(gpfMock.Object, ResourceGraph, options);
            var traversalHelper = new NodeNavigator(ResourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, ResourceGraph);

            return new TestObjectsC<TPrimary, TFirstSecondary, TSecondSecondary>(constraintsMock, hookExecutor, primaryResource, firstSecondaryResource,
                secondSecondaryResource);
        }

        protected IHooksDiscovery<TResource> SetDiscoverableHooks<TResource>(ResourceHook[] implementedHooks, params ResourceHook[] enableDbValuesHooks)
            where TResource : class, IIdentifiable<int>
        {
            var mock = new Mock<IHooksDiscovery<TResource>>();
            mock.Setup(discovery => discovery.ImplementedHooks).Returns(implementedHooks);

            if (!enableDbValuesHooks.Any())
            {
                mock.Setup(discovery => discovery.DatabaseValuesDisabledHooks).Returns(enableDbValuesHooks);
            }

            mock.Setup(discovery => discovery.DatabaseValuesEnabledHooks)
                .Returns(ResourceHook.BeforeImplicitUpdateRelationship.AsEnumerable().Concat(enableDbValuesHooks).ToArray());

            return mock.Object;
        }

        protected void VerifyNoOtherCalls(params dynamic[] resourceMocks)
        {
            foreach (dynamic mock in resourceMocks)
            {
                mock.VerifyNoOtherCalls();
            }
        }

        protected DbContextOptions<AppDbContext> InitInMemoryDb(Action<DbContext> seeder)
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("repository_mock").Options;

            using var context = new AppDbContext(options);

            seeder(context);
            ResolveInverseRelationships(context);

            return options;
        }

        private void MockHooks<TModel>(Mock<IResourceHookContainer<TModel>> resourceDefinition)
            where TModel : class, IIdentifiable<int>
        {
            resourceDefinition.Setup(rd => rd.BeforeCreate(It.IsAny<IResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, _) => resources).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeRead(It.IsAny<ResourcePipeline>(), It.IsAny<bool>(), It.IsAny<string>())).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeUpdate(It.IsAny<IDiffableResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<DiffableResourceHashSet<TModel>, ResourcePipeline>((resources, _) => resources).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeDelete(It.IsAny<IResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, _) => resources).Verifiable();

            resourceDefinition
                .Setup(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<TModel>>(),
                    It.IsAny<ResourcePipeline>())).Returns<IEnumerable<string>, IRelationshipsDictionary<TModel>, ResourcePipeline>((ids, _, __) => ids)
                .Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeImplicitUpdateRelationship(It.IsAny<IRelationshipsDictionary<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Verifiable();

            resourceDefinition.Setup(rd => rd.OnReturn(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, _) => resources).Verifiable();

            resourceDefinition.Setup(rd => rd.AfterCreate(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>())).Verifiable();
            resourceDefinition.Setup(rd => rd.AfterRead(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>(), It.IsAny<bool>())).Verifiable();
            resourceDefinition.Setup(rd => rd.AfterUpdate(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>())).Verifiable();
            resourceDefinition.Setup(rd => rd.AfterDelete(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>(), It.IsAny<bool>())).Verifiable();
        }

        private void SetupProcessorFactoryForResourceDefinition<TModel>(Mock<IGenericServiceFactory> processorFactory,
            IResourceHookContainer<TModel> modelResource, IHooksDiscovery<TModel> discovery, AppDbContext dbContext = null, IResourceGraph resourceGraph = null)
            where TModel : class, IIdentifiable<int>
        {
            processorFactory.Setup(factory => factory.Get<IResourceHookContainer>(typeof(ResourceHooksDefinition<>), typeof(TModel))).Returns(modelResource);

            processorFactory.Setup(factory => factory.Get<IHooksDiscovery>(typeof(IHooksDiscovery<>), typeof(TModel))).Returns(discovery);

            if (dbContext != null)
            {
                Type idType = ObjectFactory.GetIdType(typeof(TModel));

                if (idType == typeof(int))
                {
                    IResourceReadRepository<TModel, int> repo = CreateTestRepository<TModel>(dbContext, resourceGraph);

                    processorFactory.Setup(factory =>
                        factory.Get<IResourceReadRepository<TModel, int>>(typeof(IResourceReadRepository<,>), typeof(TModel), typeof(int))).Returns(repo);
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
            IServiceProvider serviceProvider = ((IInfrastructure<IServiceProvider>)dbContext).Instance;
            var resourceFactory = new ResourceFactory(serviceProvider);
            IDbContextResolver resolver = CreateTestDbResolver(dbContext);
            var targetedFields = new TargetedFields();

            return new EntityFrameworkCoreRepository<TModel, int>(targetedFields, resolver, resourceGraph, resourceFactory,
                Enumerable.Empty<IQueryConstraintProvider>(), NullLoggerFactory.Instance);
        }

        private IDbContextResolver CreateTestDbResolver(AppDbContext dbContext)
        {
            var mock = new Mock<IDbContextResolver>();
            mock.Setup(resolver => resolver.GetContext()).Returns(dbContext);
            return mock.Object;
        }

        private void ResolveInverseRelationships(AppDbContext context)
        {
            IEnumerable<DbContextResolver<AppDbContext>> dbContextResolvers = new DbContextResolver<AppDbContext>(context).AsEnumerable();
            var inverseRelationships = new InverseNavigationResolver(ResourceGraph, dbContextResolvers);
            inverseRelationships.Resolve();
        }

        private Mock<IResourceHookContainer<TModel>> CreateResourceDefinition<TModel>()
            where TModel : class, IIdentifiable<int>
        {
            var resourceDefinition = new Mock<IResourceHookContainer<TModel>>();
            MockHooks(resourceDefinition);
            return resourceDefinition;
        }

        protected IncludeExpression ToIncludeExpression(params string[] includePaths)
        {
            var relationshipChains = new List<ResourceFieldChainExpression>();

            foreach (string includePath in includePaths)
            {
                ResourceFieldChainExpression relationshipChain = GetRelationshipsInPath(includePath);
                relationshipChains.Add(relationshipChain);
            }

            return IncludeChainConverter.FromRelationshipChains(relationshipChains);
        }

        private ResourceFieldChainExpression GetRelationshipsInPath(string includePath)
        {
            ResourceContext resourceContext = ResourceGraph.GetResourceContext<TodoItem>();
            var relationships = new List<RelationshipAttribute>();

            foreach (string relationshipName in includePath.Split('.'))
            {
                RelationshipAttribute relationship = resourceContext.Relationships.Single(nextRelationship => nextRelationship.PublicName == relationshipName);

                relationships.Add(relationship);

                resourceContext = ResourceGraph.GetResourceContext(relationship.RightType);
            }

            return new ResourceFieldChainExpression(relationships);
        }

        protected IEnumerable<IQueryConstraintProvider> Wrap(IncludeExpression includeExpression)
        {
            var expressionsInScope = new List<ExpressionInScope>
            {
                new ExpressionInScope(null, includeExpression)
            };

            var mock = new Mock<IQueryConstraintProvider>();
            mock.Setup(provider => provider.GetConstraints()).Returns(expressionsInScope);

            IQueryConstraintProvider includeConstraintProvider = mock.Object;
            return includeConstraintProvider.AsEnumerable();
        }

        private sealed class TestMocks
        {
            public Mock<ITargetedFields> TargetedFieldsMock { get; }
            public Mock<IEnumerable<IQueryConstraintProvider>> ConstraintsMock { get; }
            public Mock<IGenericServiceFactory> GenericServiceFactoryMock { get; }
            public IJsonApiOptions Options { get; }

            public TestMocks(Mock<ITargetedFields> targetedFieldsMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock,
                Mock<IGenericServiceFactory> genericServiceFactoryMock, IJsonApiOptions options)
            {
                TargetedFieldsMock = targetedFieldsMock;
                ConstraintsMock = constraintsMock;
                GenericServiceFactoryMock = genericServiceFactoryMock;
                Options = options;
            }

            public void Deconstruct(out Mock<ITargetedFields> targetedFieldsMock, out Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock,
                out Mock<IGenericServiceFactory> genericServiceFactoryMock, out IJsonApiOptions optionsMock)
            {
                targetedFieldsMock = TargetedFieldsMock;
                constraintsMock = ConstraintsMock;
                genericServiceFactoryMock = GenericServiceFactoryMock;
                optionsMock = Options;
            }
        }

        protected sealed class TestObjectsA<TPrimary>
            where TPrimary : class, IIdentifiable<int>
        {
            public IResourceHookExecutor HookExecutor { get; }
            public Mock<IResourceHookContainer<TPrimary>> PrimaryResourceContainerMock { get; }

            public TestObjectsA(IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TPrimary>> primaryResourceContainerMock)
            {
                HookExecutor = hookExecutor;
                PrimaryResourceContainerMock = primaryResourceContainerMock;
            }

            public void Deconstruct(out IResourceHookExecutor hookExecutor, out Mock<IResourceHookContainer<TPrimary>> primaryResourceContainerMock)
            {
                hookExecutor = HookExecutor;
                primaryResourceContainerMock = PrimaryResourceContainerMock;
            }
        }

        protected sealed class TestObjectsB<TPrimary, TSecondary>
            where TPrimary : class, IIdentifiable<int>
            where TSecondary : class, IIdentifiable<int>
        {
            public Mock<IEnumerable<IQueryConstraintProvider>> ConstraintsMock { get; }
            public Mock<ITargetedFields> TargetedFieldsMock { get; }
            public IResourceHookExecutor HookExecutor { get; }
            public Mock<IResourceHookContainer<TPrimary>> PrimaryResourceContainerMock { get; }
            public Mock<IResourceHookContainer<TSecondary>> SecondaryResourceContainerMock { get; }

            public TestObjectsB(Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<ITargetedFields> targetedFieldsMock,
                IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TPrimary>> primaryResourceContainerMock,
                Mock<IResourceHookContainer<TSecondary>> secondaryResourceContainerMock)
            {
                ConstraintsMock = constraintsMock;
                TargetedFieldsMock = targetedFieldsMock;
                HookExecutor = hookExecutor;
                PrimaryResourceContainerMock = primaryResourceContainerMock;
                SecondaryResourceContainerMock = secondaryResourceContainerMock;
            }

            public void Deconstruct(out Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, out Mock<ITargetedFields> ufMock,
                out IResourceHookExecutor hookExecutor, out Mock<IResourceHookContainer<TPrimary>> primaryResource,
                out Mock<IResourceHookContainer<TSecondary>> secondaryResource)
            {
                constraintsMock = ConstraintsMock;
                ufMock = TargetedFieldsMock;
                hookExecutor = HookExecutor;
                primaryResource = PrimaryResourceContainerMock;
                secondaryResource = SecondaryResourceContainerMock;
            }
        }

        protected sealed class TestObjectsC<TPrimary, TFirstSecondary, TSecondSecondary>
            where TPrimary : class, IIdentifiable<int>
            where TFirstSecondary : class, IIdentifiable<int>
            where TSecondSecondary : class, IIdentifiable<int>
        {
            public Mock<IEnumerable<IQueryConstraintProvider>> ConstraintsMock { get; }
            public IResourceHookExecutor HookExecutor { get; }
            public Mock<IResourceHookContainer<TPrimary>> PrimaryResourceContainerMock { get; }
            public Mock<IResourceHookContainer<TFirstSecondary>> FirstSecondaryResourceContainerMock { get; }
            public Mock<IResourceHookContainer<TSecondSecondary>> SecondSecondaryResourceContainerMock { get; }

            public TestObjectsC(Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, IResourceHookExecutor hookExecutor,
                Mock<IResourceHookContainer<TPrimary>> primaryResourceContainerMock,
                Mock<IResourceHookContainer<TFirstSecondary>> firstSecondaryResourceContainerMock,
                Mock<IResourceHookContainer<TSecondSecondary>> secondSecondaryResourceContainerMock)
            {
                ConstraintsMock = constraintsMock;
                HookExecutor = hookExecutor;
                PrimaryResourceContainerMock = primaryResourceContainerMock;
                FirstSecondaryResourceContainerMock = firstSecondaryResourceContainerMock;
                SecondSecondaryResourceContainerMock = secondSecondaryResourceContainerMock;
            }

            public void Deconstruct(out Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, out IResourceHookExecutor hookExecutor,
                out Mock<IResourceHookContainer<TPrimary>> primaryResourceContainerMock,
                out Mock<IResourceHookContainer<TFirstSecondary>> firstSecondaryResourceContainerMock,
                out Mock<IResourceHookContainer<TSecondSecondary>> secondSecondaryResourceContainerMock)
            {
                constraintsMock = ConstraintsMock;
                hookExecutor = HookExecutor;
                primaryResourceContainerMock = PrimaryResourceContainerMock;
                firstSecondaryResourceContainerMock = FirstSecondaryResourceContainerMock;
                secondSecondaryResourceContainerMock = SecondSecondaryResourceContainerMock;
            }
        }
    }
}
