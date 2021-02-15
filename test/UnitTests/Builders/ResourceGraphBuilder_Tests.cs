using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.Builders
{
    public sealed class ResourceGraphBuilder_Tests
    {
        private sealed class NonDbResource : Identifiable { }

        private sealed class DbResource : Identifiable { }

        private class TestContext : DbContext
        {
            public DbSet<DbResource> DbResources { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }
        }

        [Fact]
        public void Can_Build_ResourceGraph_Using_Builder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestContext>();
            
            services.AddJsonApi<TestContext>(resources: builder => builder.Add<NonDbResource>("nonDbResources"));

            // Act
            var container = services.BuildServiceProvider();

            // Assert
            var resourceGraph = container.GetRequiredService<IResourceGraph>();
            var dbResource = resourceGraph.GetResourceContext("dbResources");
            var nonDbResource = resourceGraph.GetResourceContext("nonDbResources");
            Assert.Equal(typeof(DbResource), dbResource.ResourceType);
            Assert.Equal(typeof(NonDbResource), nonDbResource.ResourceType);
        }

        [Fact]
        public void Resources_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();

            // Act
            var resourceGraph = builder.Build();

            // Assert
            var resource = resourceGraph.GetResourceContext(typeof(TestResource));
            Assert.Equal("testResources", resource.PublicName);
        }

        [Fact]
        public void Attrs_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();

            // Act
            var resourceGraph = builder.Build();

            // Assert
            var resource = resourceGraph.GetResourceContext(typeof(TestResource));
            Assert.Contains(resource.Attributes, (i) => i.PublicName == "compoundAttribute");
        }

        [Fact]
        public void Relationships_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();

            // Act
            var resourceGraph = builder.Build();

            // Assert
            var resource = resourceGraph.GetResourceContext(typeof(TestResource));
            Assert.Equal("relatedResource", resource.Relationships.Single(r => r is HasOneAttribute).PublicName);
            Assert.Equal("relatedResources", resource.Relationships.Single(r => !(r is HasOneAttribute)).PublicName);
        }

        public sealed class TestResource : Identifiable
        {
            [Attr] 
            public string CompoundAttribute { get; set; }
            
            [HasOne] 
            public RelatedResource RelatedResource { get; set; }
            
            [HasMany] 
            public ISet<RelatedResource> RelatedResources { get; set; }
        }

        public class RelatedResource : Identifiable { }
    }
}
