using GettingStarted.Models;
using GettingStarted.ResourceDefinitionExample;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
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
        private readonly ResourceGraphBuilder _graphBuilder = new ResourceGraphBuilder();

        public ServiceDiscoveryFacadeTests()
        {
            var contextMock = new Mock<DbContext>();
            var dbResolverMock = new Mock<IDbContextResolver>();
            dbResolverMock.Setup(m => m.GetContext()).Returns(new Mock<DbContext>().Object);
            TestModelRepository._dbContextResolver = dbResolverMock.Object;
        }

        private ServiceDiscoveryFacade _facade => new ServiceDiscoveryFacade(_services, _graphBuilder);

        [Fact]
        public void AddAssembly_Adds_All_Resources_To_Graph()
        {
            // arrange, act
            _facade.AddAssembly(typeof(Person).Assembly);

            // assert
            var graph = _graphBuilder.Build();
            var personResource = graph.GetContextEntity(typeof(Person));
            var articleResource = graph.GetContextEntity(typeof(Article));
            var modelResource = graph.GetContextEntity(typeof(Model));

            Assert.NotNull(personResource);
            Assert.NotNull(articleResource);
            Assert.NotNull(modelResource);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Resources_To_Graph()
        {
            // arrange, act
            _facade.AddCurrentAssembly();

            // assert
            var graph = _graphBuilder.Build();
            var testModelResource = graph.GetContextEntity(typeof(TestModel));
            Assert.NotNull(testModelResource);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Services_To_Container()
        {
            // arrange, act
            _services.AddSingleton<IJsonApiOptions>(new JsonApiOptions());

            _services.AddScoped((_) => new Mock<ILinkBuilder>().Object);
            _services.AddScoped((_) => new Mock<IRequestContext>().Object);
            _services.AddScoped((_) => new Mock<IPageQueryService>().Object);
            _services.AddScoped((_) => new Mock<IResourceGraph>().Object);
            _facade.AddCurrentAssembly();

            // assert
            var services = _services.BuildServiceProvider();
            var service = services.GetService<IResourceService<TestModel>>();
            Assert.IsType<TestModelService>(service);
        }

        [Fact]
        public void AddCurrentAssembly_Adds_Repositories_To_Container()
        {
            // arrange, act
            _facade.AddCurrentAssembly();

            // assert
            var services = _services.BuildServiceProvider();
            Assert.IsType<TestModelRepository>(services.GetService<IEntityRepository<TestModel>>());
        }

        public class TestModel : Identifiable { }

        public class TestModelService : EntityResourceService<TestModel>
        {
            private static IEntityRepository<TestModel> _repo = new Mock<IEntityRepository<TestModel>>().Object;
            private static IJsonApiContext _jsonApiContext = new Mock<IJsonApiContext>().Object;

            public TestModelService(
                IEntityRepository<TestModel> repository,
                IJsonApiOptions options,
                IRequestContext currentRequest,
                IPageQueryService pageService,
                IResourceGraph resourceGraph,
                ILoggerFactory loggerFactory = null,
                IResourceHookExecutor hookExecutor = null) : base(repository, options, currentRequest, pageService, resourceGraph, loggerFactory, hookExecutor)
            {
            }
        }

        public class TestModelRepository : DefaultEntityRepository<TestModel>
        {
            internal static IDbContextResolver _dbContextResolver;
            private static IJsonApiContext _jsonApiContext = new Mock<IJsonApiContext>().Object;
            public TestModelRepository() : base(_jsonApiContext, _dbContextResolver) { }
        }
    }
}
