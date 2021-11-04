using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
            services.AddDbContext<TestDbContext>();

            services.AddJsonApi<TestDbContext>(resources: builder => builder.Add<NonDbResource>("nonDbResources"));

            // Act
            ServiceProvider container = services.BuildServiceProvider();

            // Assert
            var resourceGraph = container.GetRequiredService<IResourceGraph>();

            ResourceType dbResourceType = resourceGraph.GetResourceType("dbResources");
            dbResourceType.ClrType.Should().Be(typeof(DbResource));

            ResourceType nonDbResourceType = resourceGraph.GetResourceType("nonDbResources");
            nonDbResourceType.ClrType.Should().Be(typeof(NonDbResource));
        }

        [Fact]
        public void Resources_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();
            builder.Add<RelatedResource>();

            // Act
            IResourceGraph resourceGraph = builder.Build();

            // Assert
            ResourceType testResourceType = resourceGraph.GetResourceType<TestResource>();
            testResourceType.PublicName.Should().Be("testResources");
        }

        [Fact]
        public void Attrs_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();
            builder.Add<RelatedResource>();

            // Act
            IResourceGraph resourceGraph = builder.Build();

            // Assert
            ResourceType testResourceType = resourceGraph.GetResourceType<TestResource>();
            testResourceType.Attributes.Should().ContainSingle(attribute => attribute.PublicName == "compoundAttribute");
        }

        [Fact]
        public void Relationships_Without_Names_Specified_Will_Use_Configured_Formatter()
        {
            // Arrange
            var builder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
            builder.Add<TestResource>();
            builder.Add<RelatedResource>();

            // Act
            IResourceGraph resourceGraph = builder.Build();

            // Assert
            ResourceType testResourceType = resourceGraph.GetResourceType<TestResource>();

            testResourceType.Relationships.Should().HaveCount(2);

            testResourceType.Relationships.ElementAt(0).Should().BeOfType<HasOneAttribute>();
            testResourceType.Relationships.ElementAt(0).PublicName.Should().Be("relatedResource");

            testResourceType.Relationships.ElementAt(1).Should().BeOfType<HasManyAttribute>();
            testResourceType.Relationships.ElementAt(1).PublicName.Should().Be("relatedResources");
        }

        private sealed class NonDbResource : Identifiable<int>
        {
        }

        private sealed class DbResource : Identifiable<int>
        {
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        private sealed class TestDbContext : DbContext
        {
            public DbSet<DbResource> DbResources => Set<DbResource>();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class TestResource : Identifiable<int>
        {
            [Attr]
            public string? CompoundAttribute { get; set; }

            [HasOne]
            public RelatedResource? RelatedResource { get; set; }

            [HasMany]
            public ISet<RelatedResource> RelatedResources { get; set; } = new HashSet<RelatedResource>();
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class RelatedResource : Identifiable<int>
        {
            [Attr]
            public string? Unused { get; set; }
        }
    }
}
