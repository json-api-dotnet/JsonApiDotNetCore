using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
            var dbResource = contextGraph.GetContextEntity("db-resources").EntityType;
            var nonDbResource = contextGraph.GetContextEntity("non-db-resources").EntityType;
            Assert.Equal(typeof(DbResource), dbResource);
            Assert.Equal(typeof(NonDbResource), nonDbResource);
        }
    }
}
