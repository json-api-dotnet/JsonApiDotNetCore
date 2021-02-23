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
        private (Mock<ITargetedFields>, Mock<IEnumerable<IQueryConstraintProvider>>, Mock<IGenericServiceFactory>, IJsonApiOptions) CreateMocks()
        {
            var pfMock = new Mock<IGenericServiceFactory>();
            var ufMock = new Mock<ITargetedFields>();

            var constraintsMock = new Mock<IEnumerable<IQueryConstraintProvider>>();
            constraintsMock.Setup(x => x.GetEnumerator()).Returns(Enumerable.Empty<IQueryConstraintProvider>().GetEnumerator());

            var optionsMock = new JsonApiOptions
            {
                LoadDatabaseValues = false
            };

            return (ufMock, constraintsMock, pfMock, optionsMock);
        }

        internal (Mock<IEnumerable<IQueryConstraintProvider>>, ResourceHookExecutor, Mock<IResourceHookContainer<TPrimary>>) CreateTestObjects<TPrimary>(
            IHooksDiscovery<TPrimary> primaryDiscovery = null)
            where TPrimary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding ImplementedHooks discovery instance
            Mock<IResourceHookContainer<TPrimary>> primaryResource = CreateResourceDefinition(primaryDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            (Mock<ITargetedFields> ufMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<IGenericServiceFactory> gpfMock,
                IJsonApiOptions options) = CreateMocks();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery);

            var execHelper = new HookExecutorHelper(gpfMock.Object, ResourceGraph, options);
            var traversalHelper = new TraversalHelper(ResourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, ResourceGraph);

            return (constraintsMock, hookExecutor, primaryResource);
        }

        protected (Mock<IEnumerable<IQueryConstraintProvider>>, Mock<ITargetedFields>, IResourceHookExecutor, Mock<IResourceHookContainer<TPrimary>>,
            Mock<IResourceHookContainer<TSecondary>>) CreateTestObjects<TPrimary, TSecondary>(IHooksDiscovery<TPrimary> primaryDiscovery = null,
                IHooksDiscovery<TSecondary> secondaryDiscovery = null, DbContextOptions<AppDbContext> repoDbContextOptions = null)
            where TPrimary : class, IIdentifiable<int>
            where TSecondary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            Mock<IResourceHookContainer<TPrimary>> primaryResource = CreateResourceDefinition(primaryDiscovery);
            Mock<IResourceHookContainer<TSecondary>> secondaryResource = CreateResourceDefinition(secondaryDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            (Mock<ITargetedFields> ufMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<IGenericServiceFactory> gpfMock,
                IJsonApiOptions options) = CreateMocks();

            AppDbContext dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TPrimary>().Add<TSecondary>().Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondaryResource.Object, secondaryDiscovery, dbContext, resourceGraph);

            var execHelper = new HookExecutorHelper(gpfMock.Object, ResourceGraph, options);
            var traversalHelper = new TraversalHelper(ResourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, ResourceGraph);

            return (constraintsMock, ufMock, hookExecutor, primaryResource, secondaryResource);
        }

        protected (Mock<IEnumerable<IQueryConstraintProvider>>, IResourceHookExecutor, Mock<IResourceHookContainer<TPrimary>>,
            Mock<IResourceHookContainer<TFirstSecondary>>, Mock<IResourceHookContainer<TSecondSecondary>>)
            CreateTestObjects<TPrimary, TFirstSecondary, TSecondSecondary>(IHooksDiscovery<TPrimary> primaryDiscovery = null,
                IHooksDiscovery<TFirstSecondary> firstSecondaryDiscovery = null, IHooksDiscovery<TSecondSecondary> secondSecondaryDiscovery = null,
                DbContextOptions<AppDbContext> repoDbContextOptions = null)
            where TPrimary : class, IIdentifiable<int>
            where TFirstSecondary : class, IIdentifiable<int>
            where TSecondSecondary : class, IIdentifiable<int>
        {
            // creates the resource definition mock and corresponding for a given set of discoverable hooks
            Mock<IResourceHookContainer<TPrimary>> primaryResource = CreateResourceDefinition(primaryDiscovery);
            Mock<IResourceHookContainer<TFirstSecondary>> firstSecondaryResource = CreateResourceDefinition(firstSecondaryDiscovery);
            Mock<IResourceHookContainer<TSecondSecondary>> secondSecondaryResource = CreateResourceDefinition(secondSecondaryDiscovery);

            // mocking the genericServiceFactory and JsonApiContext and wiring them up.
            (Mock<ITargetedFields> ufMock, Mock<IEnumerable<IQueryConstraintProvider>> constraintsMock, Mock<IGenericServiceFactory> gpfMock,
                IJsonApiOptions options) = CreateMocks();

            AppDbContext dbContext = repoDbContextOptions != null ? new AppDbContext(repoDbContextOptions) : null;

            IResourceGraph resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TPrimary>().Add<TFirstSecondary>()
                .Add<TSecondSecondary>().Build();

            SetupProcessorFactoryForResourceDefinition(gpfMock, primaryResource.Object, primaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, firstSecondaryResource.Object, firstSecondaryDiscovery, dbContext, resourceGraph);
            SetupProcessorFactoryForResourceDefinition(gpfMock, secondSecondaryResource.Object, secondSecondaryDiscovery, dbContext, resourceGraph);

            var execHelper = new HookExecutorHelper(gpfMock.Object, ResourceGraph, options);
            var traversalHelper = new TraversalHelper(ResourceGraph, ufMock.Object);
            var hookExecutor = new ResourceHookExecutor(execHelper, traversalHelper, ufMock.Object, constraintsMock.Object, ResourceGraph);

            return (constraintsMock, hookExecutor, primaryResource, firstSecondaryResource, secondSecondaryResource);
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
                .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, context) => resources).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeRead(It.IsAny<ResourcePipeline>(), It.IsAny<bool>(), It.IsAny<string>())).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeUpdate(It.IsAny<IDiffableResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<DiffableResourceHashSet<TModel>, ResourcePipeline>((resources, context) => resources).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeDelete(It.IsAny<IResourceHashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, context) => resources).Verifiable();

            resourceDefinition
                .Setup(rd => rd.BeforeUpdateRelationship(It.IsAny<HashSet<string>>(), It.IsAny<IRelationshipsDictionary<TModel>>(),
                    It.IsAny<ResourcePipeline>()))
                .Returns<IEnumerable<string>, IRelationshipsDictionary<TModel>, ResourcePipeline>((ids, context, helper) => ids).Verifiable();

            resourceDefinition.Setup(rd => rd.BeforeImplicitUpdateRelationship(It.IsAny<IRelationshipsDictionary<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Verifiable();

            resourceDefinition.Setup(rd => rd.OnReturn(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>()))
                .Returns<IEnumerable<TModel>, ResourcePipeline>((resources, context) => resources).Verifiable();

            resourceDefinition.Setup(rd => rd.AfterCreate(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>())).Verifiable();
            resourceDefinition.Setup(rd => rd.AfterRead(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>(), It.IsAny<bool>())).Verifiable();
            resourceDefinition.Setup(rd => rd.AfterUpdate(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>())).Verifiable();
            resourceDefinition.Setup(rd => rd.AfterDelete(It.IsAny<HashSet<TModel>>(), It.IsAny<ResourcePipeline>(), It.IsAny<bool>())).Verifiable();
        }

        private void SetupProcessorFactoryForResourceDefinition<TModel>(Mock<IGenericServiceFactory> processorFactory,
            IResourceHookContainer<TModel> modelResource, IHooksDiscovery<TModel> discovery, AppDbContext dbContext = null, IResourceGraph resourceGraph = null)
            where TModel : class, IIdentifiable<int>
        {
            processorFactory.Setup(c => c.Get<IResourceHookContainer>(typeof(ResourceHooksDefinition<>), typeof(TModel))).Returns(modelResource);

            processorFactory.Setup(c => c.Get<IHooksDiscovery>(typeof(IHooksDiscovery<>), typeof(TModel))).Returns(discovery);

            if (dbContext != null)
            {
                Type idType = TypeHelper.GetIdType(typeof(TModel));

                if (idType == typeof(int))
                {
                    IResourceReadRepository<TModel, int> repo = CreateTestRepository<TModel>(dbContext, resourceGraph);

                    processorFactory.Setup(c => c.Get<IResourceReadRepository<TModel, int>>(typeof(IResourceReadRepository<,>), typeof(TModel), typeof(int)))
                        .Returns(repo);
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
            mock.Setup(r => r.GetContext()).Returns(dbContext);
            return mock.Object;
        }

        private void ResolveInverseRelationships(AppDbContext context)
        {
            IEnumerable<DbContextResolver<AppDbContext>> dbContextResolvers = new DbContextResolver<AppDbContext>(context).AsEnumerable();
            var inverseRelationships = new InverseNavigationResolver(ResourceGraph, dbContextResolvers);
            inverseRelationships.Resolve();
        }

        private Mock<IResourceHookContainer<TModel>> CreateResourceDefinition<TModel>(IHooksDiscovery<TModel> discovery)
            where TModel : class, IIdentifiable<int>
        {
            var resourceDefinition = new Mock<IResourceHookContainer<TModel>>();
            MockHooks(resourceDefinition);
            return resourceDefinition;
        }

        protected List<List<RelationshipAttribute>> GetIncludedRelationshipsChains(params string[] chains)
        {
            var parsedChains = new List<List<RelationshipAttribute>>();

            foreach (string chain in chains)
            {
                parsedChains.Add(GetIncludedRelationshipsChain(chain));
            }

            return parsedChains;
        }

        protected List<RelationshipAttribute> GetIncludedRelationshipsChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            ResourceContext resourceContext = ResourceGraph.GetResourceContext<TodoItem>();
            string[] splitPath = chain.Split('.');

            foreach (string requestedRelationship in splitPath)
            {
                RelationshipAttribute relationship = resourceContext.Relationships.Single(r => r.PublicName == requestedRelationship);
                parsedChain.Add(relationship);
                resourceContext = ResourceGraph.GetResourceContext(relationship.RightType);
            }

            return parsedChain;
        }

        protected IEnumerable<IQueryConstraintProvider> ConvertInclusionChains(List<List<RelationshipAttribute>> inclusionChains)
        {
            var expressionsInScope = new List<ExpressionInScope>();

            if (inclusionChains != null)
            {
                List<ResourceFieldChainExpression> chains = inclusionChains.Select(relationships => new ResourceFieldChainExpression(relationships)).ToList();
                IncludeExpression includeExpression = IncludeChainConverter.FromRelationshipChains(chains);
                expressionsInScope.Add(new ExpressionInScope(null, includeExpression));
            }

            var mock = new Mock<IQueryConstraintProvider>();
            mock.Setup(x => x.GetConstraints()).Returns(expressionsInScope);

            IQueryConstraintProvider includeConstraintProvider = mock.Object;
            return includeConstraintProvider.AsEnumerable();
        }
    }
}
