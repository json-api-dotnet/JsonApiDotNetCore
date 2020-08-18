using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.RequestServices.Contracts;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Services.Contract;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DiscoveryTests
{
    public sealed class ServiceDiscoveryFacadeTests
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ResourceGraphBuilder _resourceGraphBuilder;

        public ServiceDiscoveryFacadeTests()
        {
            var options = new JsonApiOptions();

            var dbResolverMock = new Mock<IDbContextResolver>();
            dbResolverMock.Setup(m => m.GetContext()).Returns(new Mock<DbContext>().Object);
            TestModelRepository._dbContextResolver = dbResolverMock.Object;

            _services.AddSingleton<IJsonApiOptions>(options);
            _services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            _services.AddScoped(_ => new Mock<ILinkBuilder>().Object);
            _services.AddScoped(_ => new Mock<IJsonApiRequest>().Object);
            _services.AddScoped(_ => new Mock<ITargetedFields>().Object);
            _services.AddScoped(_ => new Mock<IResourceGraph>().Object);
            _services.AddScoped(_ => new Mock<IGenericServiceFactory>().Object);
            _services.AddScoped(_ => new Mock<IResourceContextProvider>().Object);
            _services.AddScoped(typeof(IResourceChangeTracker<>), typeof(ResourceChangeTracker<>));
            _services.AddScoped(_ => new Mock<IResourceFactory>().Object);
            _services.AddScoped(_ => new Mock<IPaginationContext>().Object);
            _services.AddScoped(_ => new Mock<IQueryLayerComposer>().Object);
            _services.AddTransient(_ => new Mock<IResourceDefinitionProvider>().Object);

            _resourceGraphBuilder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);
        }

        private ServiceDiscoveryFacade Facade => new ServiceDiscoveryFacade(_services, _resourceGraphBuilder);

        [Fact]
        public void AddAssembly_Adds_All_Resources_To_Graph()
        {
            // Arrange, act
            Facade.AddAssembly(typeof(Person).Assembly);

            // Assert
            var resourceGraph = _resourceGraphBuilder.Build();
            var personResource = resourceGraph.GetResourceContext(typeof(Person));
            var articleResource = resourceGraph.GetResourceContext(typeof(Article));

            Assert.NotNull(personResource);
            Assert.NotNull(articleResource);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Resources_To_Graph()
        {
            // Arrange, act
            Facade.AddCurrentAssembly();

            // Assert
            var resourceGraph = _resourceGraphBuilder.Build();
            var testModelResource = resourceGraph.GetResourceContext(typeof(TestModel));
            Assert.NotNull(testModelResource);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Services_To_Container()
        {
            // Arrange, act
            Facade.AddCurrentAssembly();

            // Assert
            var services = _services.BuildServiceProvider();
            var service = services.GetService<IResourceService<TestModel>>();
            Assert.IsType<TestModelService>(service);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Repositories_To_Container()
        {
            // Arrange, act
            Facade.AddCurrentAssembly();

            // Assert
            var services = _services.BuildServiceProvider();
            Assert.IsType<TestModelRepository>(services.GetService<IResourceRepository<TestModel>>());
        }

        public sealed class TestModel : Identifiable { }

        public class TestModelService : JsonApiResourceService<TestModel>
        {
            public TestModelService(
                IResourceRepository<TestModel> repository,
                IQueryLayerComposer queryLayerComposer,
                IPaginationContext paginationContext,
                IJsonApiOptions options,
                ILoggerFactory loggerFactory,
                IJsonApiRequest request,
                IResourceChangeTracker<TestModel> resourceChangeTracker,
                IResourceFactory resourceFactory,
                IResourceHookExecutor hookExecutor = null)
                : base(repository, queryLayerComposer, paginationContext, options, loggerFactory, request,
                    resourceChangeTracker, resourceFactory, hookExecutor)
            {
            }
        }

        public class TestModelRepository : EntityFrameworkCoreRepository<TestModel>
        {
            internal static IDbContextResolver _dbContextResolver;

            public TestModelRepository(
                ITargetedFields targetedFields,
                IResourceGraph resourceGraph,
                IGenericServiceFactory genericServiceFactory,
                IResourceFactory resourceFactory,
                IEnumerable<IQueryConstraintProvider> constraintProviders,
                ILoggerFactory loggerFactory)
                : base(targetedFields, _dbContextResolver, resourceGraph, genericServiceFactory, resourceFactory, constraintProviders, loggerFactory)
            { }
        }
    }
}
