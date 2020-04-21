using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

        private class Foo { }

        private class TestContext : DbContext
        {
            public DbSet<Foo> Foos { get; set; }
        }
    }

}
