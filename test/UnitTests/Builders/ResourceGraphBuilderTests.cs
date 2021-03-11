using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.Builders
{
    public sealed class ResourceGraphBuilderTests
    {
        [Fact]
        public void Can_Build_ResourceGraph_Using_Builder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestContext>();

            services.AddJsonApi<TestContext>(resources: builder => builder.Add<NonDbResource>("nonDbResources"));

            // Act
            ServiceProvider container = services.BuildServiceProvider();

            // Assert
            var resourceGraph = container.GetRequiredService<IResourceGraph>();
            ResourceContext dbResource = resourceGraph.GetResourceContext("dbResources");
            ResourceContext nonDbResource = resourceGraph.GetResourceContext("nonDbResources");
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
            IResourceGraph resourceGraph = builder.Build();

            // Assert
            ResourceContext resource = resourceGraph.GetResourceContext(typeof(TestResource));
            Assert.Equal("testResources", resource.PublicName);
        }

        [Fact]
        public void Attrs_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();

            // Act
            IResourceGraph resourceGraph = builder.Build();

            // Assert
            ResourceContext resource = resourceGraph.GetResourceContext(typeof(TestResource));
            Assert.Contains(resource.Attributes, attribute => attribute.PublicName == "compoundAttribute");
        }

        [Fact]
        public void Relationships_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();

            // Act
            IResourceGraph resourceGraph = builder.Build();

            // Assert
            ResourceContext resource = resourceGraph.GetResourceContext(typeof(TestResource));
            Assert.Equal("relatedResource", resource.Relationships.Single(relationship => relationship is HasOneAttribute).PublicName);
            Assert.Equal("relatedResources", resource.Relationships.Single(relationship => !(relationship is HasOneAttribute)).PublicName);
        }

        private sealed class NonDbResource : Identifiable
        {
        }

        private sealed class DbResource : Identifiable
        {
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class TestContext : DbContext
        {
            public DbSet<DbResource> DbResources { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class TestResource : Identifiable
        {
            [Attr]
            public string CompoundAttribute { get; set; }

            [HasOne]
            public RelatedResource RelatedResource { get; set; }

            [HasMany]
            public ISet<RelatedResource> RelatedResources { get; set; }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class RelatedResource : Identifiable
        {
            [Attr]
            public string Unused { get; set; }
        }
    }
}
