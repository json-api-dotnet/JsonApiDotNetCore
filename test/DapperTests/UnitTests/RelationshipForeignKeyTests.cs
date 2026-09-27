using DapperExample.TranslationToSql.DataModel;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DapperTests.UnitTests;

public sealed class RelationshipForeignKeyTests
{
    private readonly IResourceGraph _resourceGraph;
    private readonly FromEntitiesDataModelService _dataModelService;

    public RelationshipForeignKeyTests()
    {
        var resourceGraphBuilder = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance);
        resourceGraphBuilder.Add<TestResource, long>();
        IResourceGraph resourceGraph = resourceGraphBuilder.Build();

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<ForeignKeyTestDbContext>();
        dbContextOptionsBuilder.UseNpgsql("Host=localhost");
        DbContextOptions<ForeignKeyTestDbContext> options = dbContextOptionsBuilder.Options;
        using var dbContext = new ForeignKeyTestDbContext(options);

        var dbContextResolver = new DbContextResolver<ForeignKeyTestDbContext>(dbContext);
        var inverseNavigationResolver = new InverseNavigationResolver(resourceGraph, [dbContextResolver]);
        inverseNavigationResolver.Resolve();

        _resourceGraph = resourceGraph;
        _dataModelService = new FromEntitiesDataModelService(resourceGraph);
        _dataModelService.Initialize(dbContext);
    }

    [Theory]
    [InlineData(nameof(TestResource.OneToOptionalOneForeignKeyAtLeftSide), """
    TestResource.OneToOptionalOneForeignKeyAtLeftSide (0..1:1) -> "TestResources"."OneToOptionalOneForeignKeyAtLeftSideId"?
    """)]
    [InlineData(nameof(TestResource.ManyToOptionalOneForeignKeyAtLeftSide), """
    TestResource.ManyToOptionalOneForeignKeyAtLeftSide (0..1:*) -> "TestResources"."ManyToOptionalOneForeignKeyAtLeftSideId"?
    """)]
    [InlineData(nameof(TestResource.OptionalOneToOneForeignKeyAtRightSide), """
    TestResource.OptionalOneToOneForeignKeyAtRightSide (1:0..1) -> "TestResources"."OneToOptionalOneForeignKeyAtLeftSideId"?
    """)]
    [InlineData(nameof(TestResource.OneToOneForeignKeyAtLeftSide), """
    TestResource.OneToOneForeignKeyAtLeftSide (1:1) -> "TestResources"."OneToOneForeignKeyAtLeftSideId"
    """)]
    [InlineData(nameof(TestResource.OneToOneForeignKeyAtRightSide), """
    TestResource.OneToOneForeignKeyAtRightSide (1:1) -> "TestResources"."OneToOneForeignKeyAtLeftSideId"
    """)]
    [InlineData(nameof(TestResource.ManyToOneForeignKeyAtLeftSide), """
    TestResource.ManyToOneForeignKeyAtLeftSide (1:*) -> "TestResources"."ManyToOneForeignKeyAtLeftSideId"
    """)]
    [InlineData(nameof(TestResource.OptionalOneToManyForeignKeyAtRightSide), """
    TestResource.OptionalOneToManyForeignKeyAtRightSide (*:0..1) -> "TestResources"."ManyToOptionalOneForeignKeyAtLeftSideId"?
    """)]
    [InlineData(nameof(TestResource.OneToManyForeignKeyAtRightSide), """
    TestResource.OneToManyForeignKeyAtRightSide (*:1) -> "TestResources"."ManyToOneForeignKeyAtLeftSideId"
    """)]
    public void Can_format_foreign_key(string propertyName, string expectedText)
    {
        // Arrange
        RelationshipAttribute relationship = _resourceGraph.GetResourceType<TestResource>().GetRelationshipByPropertyName(propertyName);

        // Act
        RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(relationship);

        // Assert
        foreignKey.ToString().Should().Be(expectedText);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class TestResource : Identifiable<long>
    {
        [HasOne]
        public TestResource? OneToOptionalOneForeignKeyAtLeftSide { get; set; }

        [HasOne]
        public TestResource? ManyToOptionalOneForeignKeyAtLeftSide { get; set; }

        [HasOne]
        public TestResource? OptionalOneToOneForeignKeyAtRightSide { get; set; }

        [HasOne]
        public TestResource? OneToOneForeignKeyAtLeftSide { get; set; }

        [HasOne]
        public TestResource? OneToOneForeignKeyAtRightSide { get; set; }

        [HasOne]
        public TestResource ManyToOneForeignKeyAtLeftSide { get; set; } = null!;

        [HasMany]
        public ISet<TestResource> OptionalOneToManyForeignKeyAtRightSide { get; set; } = new HashSet<TestResource>();

        [HasMany]
        public ISet<TestResource> OneToManyForeignKeyAtRightSide { get; set; } = new HashSet<TestResource>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ForeignKeyTestDbContext(DbContextOptions<ForeignKeyTestDbContext> options)
        : DbContext(options)
    {
        public DbSet<TestResource> TestResources => Set<TestResource>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TestResource>(entity =>
            {
                // @formatter:wrap_chained_method_calls chop_always

                entity.HasOne(resource => resource.OneToOptionalOneForeignKeyAtLeftSide)
                    .WithOne(resource => resource.OptionalOneToOneForeignKeyAtRightSide)
                    .HasForeignKey<TestResource>("OneToOptionalOneForeignKeyAtLeftSideId");

                entity.HasOne(resource => resource.ManyToOptionalOneForeignKeyAtLeftSide)
                    .WithMany(resource => resource.OptionalOneToManyForeignKeyAtRightSide)
                    .HasForeignKey("ManyToOptionalOneForeignKeyAtLeftSideId");

                entity.HasOne(resource => resource.OneToOneForeignKeyAtLeftSide)
                    .WithOne(resource => resource.OneToOneForeignKeyAtRightSide)
                    .HasForeignKey<TestResource>("OneToOneForeignKeyAtLeftSideId")
                    .IsRequired();

                entity.HasOne(resource => resource.ManyToOneForeignKeyAtLeftSide)
                    .WithMany(resource => resource.OneToManyForeignKeyAtRightSide)
                    .HasForeignKey("ManyToOneForeignKeyAtLeftSideId")
                    .IsRequired();

                // @formatter:wrap_chained_method_calls restore
            });
        }
    }
}
