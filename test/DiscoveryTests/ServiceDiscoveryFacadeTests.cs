using GettingStarted.Models;
using GettingStarted.ResourceDefinitionExample;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DiscoveryTests
{
    public class ServiceDiscoveryFacadeTests
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly ResourceGraphBuilder _graphBuilder = new ResourceGraphBuilder();
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
            _facade.AddCurrentAssembly();

            // assert
            var services = _services.BuildServiceProvider();
            Assert.IsType<TestModelService>(services.GetService<IResourceService<TestModel>>());
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
            private static IJsonApiContext _jsonApiContext = new  Mock<IJsonApiContext>().Object;
            private static IJsonApiOptions _jsonApiOptions = new Mock<IJsonApiOptions>().Object;
            private static IQueryManager _queryManager = new Mock<IQueryManager>().Object;
            private static IPageManager _pageManager = new Mock<IPageManager>().Object;
            public TestModelService() : base(_jsonApiContext, _repo, _jsonApiOptions, _queryManager, _pageManager) { }
        }

        public class TestModelRepository : DefaultEntityRepository<TestModel>
        {
            private static IDbContextResolver _dbContextResolver = new  Mock<IDbContextResolver>().Object;
            private static IJsonApiContext _jsonApiContext = new  Mock<IJsonApiContext>().Object;
            public TestModelRepository() : base(_jsonApiContext, _dbContextResolver) { }
        }
    }
}
