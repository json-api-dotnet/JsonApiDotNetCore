using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests
{
    public class ContextGraphBuilder_Tests
    {
        class NonDbResource : Identifiable {}
        class DbResource : Identifiable {}
        class TestContext : DbContext {
            public DbSet<DbResource> DbResources { get; set; }
        }

        [Fact]
        public void Can_Build_ContextGraph_Using_Builder()
        {
            // arrange
            var services = new ServiceCollection();
            services.AddJsonApi<TestContext>(opt => {
                opt.BuildContextGraph(b => {
                    b.AddResource<NonDbResource>("non-db-resources");
                });
            });

            // act
            var container = services.BuildServiceProvider();

            // assert
            var contextGraph = container.GetRequiredService<IContextGraph>();
            var dbResource = contextGraph.GetContextEntity("db-resources");
            var nonDbResource = contextGraph.GetContextEntity("non-db-resources");
            Assert.Equal(typeof(DbResource), dbResource.EntityType);
            Assert.Equal(typeof(NonDbResource), nonDbResource.EntityType);
            Assert.Equal(typeof(ResourceDefinition<NonDbResource>), nonDbResource.ResourceType);
        }
    }
}
