using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Extensions.EntityFrameworkCore;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace UnitTests.Internal
{
    public class ResourceGraphBuilder_Tests
    {
        [Fact]
        public void AddDbContext_Does_Not_Throw_If_Context_Contains_Members_That_DoNot_Implement_IIdentifiable()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();

            // Act
            resourceGraphBuilder.AddDbContext<TestContext>();
            var resourceGraph = resourceGraphBuilder.Build() as ResourceGraph;

            // Assert
            Assert.Empty(resourceGraph.GetResourceContexts());
        }

        [Fact]
        public void Adding_DbContext_Members_That_DoNot_Implement_IIdentifiable_Creates_Warning()
        {
            // Arrange
            var resourceGraphBuilder = new ResourceGraphBuilder();

            // Act
            resourceGraphBuilder.AddDbContext<TestContext>();
            var resourceGraph = resourceGraphBuilder.Build() as ResourceGraph;

            // Assert
            Assert.Single(resourceGraph.ValidationResults);
            Assert.Contains(resourceGraph.ValidationResults, v => v.LogLevel == LogLevel.Warning);
        }

        private class Foo { }

        private class TestContext : DbContext
        {
            public DbSet<Foo> Foos { get; set; }
        }
    }

}
