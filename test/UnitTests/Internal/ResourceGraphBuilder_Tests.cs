using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Castle.DynamicProxy;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class ResourceGraphBuilder_Tests
    {
        [Fact]
        public void AddDbContext_Does_Not_Throw_If_Context_Contains_Members_That_Do_Not_Implement_IIdentifiable()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);

            // Act
            resourceGraphBuilder.AddResource(typeof(TestContext));
            var resourceGraph = (ResourceGraph)resourceGraphBuilder.Build();

            // Assert
            Assert.Empty(resourceGraph.GetResourceContexts());
        }

        [Fact]
        public void Adding_DbContext_Members_That_Do_Not_Implement_IIdentifiable_Logs_Warning()
        {
            // Arrange
            var loggerFactory = new FakeLoggerFactory();
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), loggerFactory);
            resourceGraphBuilder.AddResource(typeof(TestContext));

            // Act
            resourceGraphBuilder.Build();

            // Assert
            Assert.Single(loggerFactory.Logger.Messages);
            Assert.Equal(LogLevel.Warning, loggerFactory.Logger.Messages[0].LogLevel);
            Assert.Equal("Entity 'UnitTests.Internal.ResourceGraphBuilder_Tests+TestContext' does not implement 'IIdentifiable'.", loggerFactory.Logger.Messages[0].Text);
        }

        [Fact]
        public void GetResourceContext_Yields_Right_Type_For_Proxy()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.AddResource<Bar>();
            var resourceGraph = (ResourceGraph)resourceGraphBuilder.Build();
            var proxyGenerator = new ProxyGenerator();

            // Act
            var proxy = proxyGenerator.CreateClassProxy<Bar>();
            var result = resourceGraph.GetResourceContext(proxy.GetType());

            // Assert
            Assert.Equal(typeof(Bar), result.ResourceType);
        }

        [Fact]
        public void GetResourceContext_Yields_Right_Type_For_Identifiable()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            resourceGraphBuilder.AddResource<Bar>();
            var resourceGraph = (ResourceGraph)resourceGraphBuilder.Build();

            // Act
            var result = resourceGraph.GetResourceContext(typeof(Bar));

            // Assert
            Assert.Equal(typeof(Bar), result.ResourceType);
        }

        private class Foo { }

        private class TestContext : DbContext
        {
            public DbSet<Foo> Foos { get; set; }
        }

        public class Bar : Identifiable { }

    }

}
