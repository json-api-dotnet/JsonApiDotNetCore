using System.Collections.Generic;
using GettingStarted.Models;
using GettingStarted.ResourceDefinitionExample;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DiscoveryTests
{
    public class ServiceDiscoveryFacadeTests
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ResourceGraphBuilder _resourceGraphBuilder = new ResourceGraphBuilder();

        public ServiceDiscoveryFacadeTests()
        {
            var contextMock = new Mock<DbContext>();
            var dbResolverMock = new Mock<IDbContextResolver>();
            dbResolverMock.Setup(m => m.GetContext()).Returns(new Mock<DbContext>().Object);
            TestModelRepository._dbContextResolver = dbResolverMock.Object;
            _services.AddSingleton<IJsonApiOptions>(new JsonApiOptions());
            _services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            _services.AddScoped((_) => new Mock<ILinkBuilder>().Object);
            _services.AddScoped((_) => new Mock<ICurrentRequest>().Object);
            _services.AddScoped((_) => new Mock<ITargetedFields>().Object);
            _services.AddScoped((_) => new Mock<IResourceGraph>().Object);
            _services.AddScoped((_) => new Mock<IGenericServiceFactory>().Object);
            _services.AddScoped((_) => new Mock<IResourceContextProvider>().Object);
        }

        private ServiceDiscoveryFacade _facade => new ServiceDiscoveryFacade(_services, _resourceGraphBuilder);

        [Fact]
        public void AddAssembly_Adds_All_Resources_To_Graph()
        {
            // Arrange, act
            _facade.AddAssembly(typeof(Person).Assembly);

            // Assert
            var resourceGraph = _resourceGraphBuilder.Build();
            var personResource = resourceGraph.GetResourceContext(typeof(Person));
            var articleResource = resourceGraph.GetResourceContext(typeof(Article));
            var modelResource = resourceGraph.GetResourceContext(typeof(Model));

            Assert.NotNull(personResource);
            Assert.NotNull(articleResource);
            Assert.NotNull(modelResource);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Resources_To_Graph()
        {
            // Arrange, act
            _facade.AddCurrentAssembly();

            // Assert
            var resourceGraph = _resourceGraphBuilder.Build();
            var testModelResource = resourceGraph.GetResourceContext(typeof(TestModel));
            Assert.NotNull(testModelResource);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Services_To_Container()
        {
            // Arrange, act
            _facade.AddCurrentAssembly();

            // Assert
            var services = _services.BuildServiceProvider();
            var service = services.GetService<IResourceService<TestModel>>();
            Assert.IsType<TestModelService>(service);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Repositories_To_Container()
        {
            // Arrange, act
            _facade.AddCurrentAssembly();

            // Assert
            var services = _services.BuildServiceProvider();
            Assert.IsType<TestModelRepository>(services.GetService<IResourceRepository<TestModel>>());
        }

        public class TestModel : Identifiable { }

        public class TestModelService : DefaultResourceService<TestModel>
        {
            private static IResourceRepository<TestModel> _repo = new Mock<IResourceRepository<TestModel>>().Object;

            public TestModelService(IEnumerable<IQueryParameterService> queryParameters, IJsonApiOptions options,
                ILoggerFactory loggerFactory, IResourceRepository<TestModel, int> repository,
                IResourceContextProvider provider, IResourceHookExecutor hookExecutor = null)
                : base(queryParameters, options, loggerFactory, repository, provider, hookExecutor)
            {
            }
        }

        public class TestModelRepository : DefaultResourceRepository<TestModel>
        {
            internal static IDbContextResolver _dbContextResolver;

            public TestModelRepository(ITargetedFields targetedFields,
                                       IResourceGraph resourceGraph,
                                       IGenericServiceFactory genericServiceFactory)
                : base(targetedFields, _dbContextResolver, resourceGraph, genericServiceFactory, null) { }
        }
    }
}
