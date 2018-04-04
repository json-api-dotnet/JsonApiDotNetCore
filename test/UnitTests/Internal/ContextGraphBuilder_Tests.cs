using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace UnitTests.Internal
{
    public class ContextGraphBuilder_Tests
    {
        [Fact]
        public void AddDbContext_Does_Not_Throw_If_Context_Contains_Members_That_DoNot_Implement_IIdentifiable()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();

            // act
            contextGraphBuilder.AddDbContext<TestContext>();
            var contextGraph = contextGraphBuilder.Build() as ContextGraph;

            // assert
            Assert.Empty(contextGraph.Entities);
        }

        [Fact]
        public void Adding_DbContext_Members_That_DoNot_Implement_IIdentifiable_Creates_Warning()
        {
            // arrange
            var contextGraphBuilder = new ContextGraphBuilder();

            // act
            contextGraphBuilder.AddDbContext<TestContext>();
            var contextGraph = contextGraphBuilder.Build() as ContextGraph;

            // assert
            Assert.Equal(1, contextGraph.ValidationResults.Count);
            Assert.Contains(contextGraph.ValidationResults, v => v.LogLevel == LogLevel.Warning);
        }

        private class Foo { }

        private class TestContext : DbContext
        {
            public DbSet<Foo> Foos { get; set; }
        }
    }

}
