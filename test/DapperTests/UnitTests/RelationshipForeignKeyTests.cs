using DapperExample.TranslationToSql.DataModel;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
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
    [InlineData(nameof(TestResource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull), """
    TestResource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull (0..1:1) -> "TestResources"."OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNullId"? (OnDelete=ClientSetNull)
    """)]
    [InlineData(nameof(TestResource.OptionalOneToOneForeignKeyAtRightSideOnDeleteClientSetNull), """
    TestResource.OptionalOneToOneForeignKeyAtRightSideOnDeleteClientSetNull (1:0..1) -> "TestResources"."OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNullId"? (OnDelete=ClientSetNull)
    """)]
    [InlineData(nameof(TestResource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull), """
    TestResource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull (0..1:1) -> "TestResources"."OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNullId"?
    """)]
    [InlineData(nameof(TestResource.OptionalOneToOneForeignKeyAtRightSideOnDeleteSetNull), """
    TestResource.OptionalOneToOneForeignKeyAtRightSideOnDeleteSetNull (1:0..1) -> "TestResources"."OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNullId"?
    """)]
    [InlineData(nameof(TestResource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull), """
    TestResource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull (0..1:*) -> "TestResources"."ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNullId"? (OnDelete=ClientSetNull)
    """)]
    [InlineData(nameof(TestResource.OptionalOneToManyForeignKeyAtRightSideOnDeleteClientSetNull), """
    TestResource.OptionalOneToManyForeignKeyAtRightSideOnDeleteClientSetNull (*:0..1) -> "TestResources"."ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNullId"? (OnDelete=ClientSetNull)
    """)]
    [InlineData(nameof(TestResource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull), """
    TestResource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull (0..1:*) -> "TestResources"."ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNullId"?
    """)]
    [InlineData(nameof(TestResource.OptionalOneToManyForeignKeyAtRightSideOnDeleteSetNull), """
    TestResource.OptionalOneToManyForeignKeyAtRightSideOnDeleteSetNull (*:0..1) -> "TestResources"."ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNullId"?
    """)]
    [InlineData(nameof(TestResource.OneToOneForeignKeyAtLeftSideOnDeleteCascade), """
    TestResource.OneToOneForeignKeyAtLeftSideOnDeleteCascade (1:1) -> "TestResources"."OneToOneForeignKeyAtLeftSideOnDeleteCascadeId"
    """)]
    [InlineData(nameof(TestResource.OneToOneForeignKeyAtRightSideOnDeleteCascade), """
    TestResource.OneToOneForeignKeyAtRightSideOnDeleteCascade (1:1) -> "TestResources"."OneToOneForeignKeyAtLeftSideOnDeleteCascadeId"
    """)]
    [InlineData(nameof(TestResource.ManyToOneForeignKeyAtLeftSideOnDeleteCascade), """
    TestResource.ManyToOneForeignKeyAtLeftSideOnDeleteCascade (1:*) -> "TestResources"."ManyToOneForeignKeyAtLeftSideOnDeleteCascadeId"
    """)]
    [InlineData(nameof(TestResource.OneToManyForeignKeyAtRightSideOnDeleteCascade), """
    TestResource.OneToManyForeignKeyAtRightSideOnDeleteCascade (*:1) -> "TestResources"."ManyToOneForeignKeyAtLeftSideOnDeleteCascadeId"
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

    [Fact]
    public void Can_get_referencing_foreign_keys_requiring_ClientSetNull_on_delete()
    {
        // Arrange
        ResourceType resourceType = _resourceGraph.GetResourceType<TestResource>();

        // Act
        IReadOnlyList<RelationshipForeignKey> foreignKeys = _dataModelService.GetReferencingForeignKeysRequiringClientSetNullOnDelete(resourceType);

        // Assert
        foreignKeys.Should().HaveCount(2);

        foreignKeys.Select(foreignKey => foreignKey.Relationship.Property.Name).Should().BeEquivalentTo(
            nameof(TestResource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull),
            nameof(TestResource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull));
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class TestResource : Identifiable<long>
    {
        [HasOne]
        public TestResource? OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull { get; set; }

        [HasOne]
        public TestResource? OptionalOneToOneForeignKeyAtRightSideOnDeleteClientSetNull { get; set; }

        [HasOne]
        public TestResource? OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull { get; set; }

        [HasOne]
        public TestResource? OptionalOneToOneForeignKeyAtRightSideOnDeleteSetNull { get; set; }

        [HasOne]
        public TestResource? ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull { get; set; }

        [HasMany]
        public ISet<TestResource> OptionalOneToManyForeignKeyAtRightSideOnDeleteClientSetNull { get; set; } = new HashSet<TestResource>();

        [HasOne]
        public TestResource? ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull { get; set; }

        [HasMany]
        public ISet<TestResource> OptionalOneToManyForeignKeyAtRightSideOnDeleteSetNull { get; set; } = new HashSet<TestResource>();

        [HasOne]
        public TestResource? OneToOneForeignKeyAtLeftSideOnDeleteCascade { get; set; }

        [HasOne]
        public TestResource? OneToOneForeignKeyAtRightSideOnDeleteCascade { get; set; }

        [HasOne]
        public TestResource ManyToOneForeignKeyAtLeftSideOnDeleteCascade { get; set; } = null!;

        [HasMany]
        public ISet<TestResource> OneToManyForeignKeyAtRightSideOnDeleteCascade { get; set; } = new HashSet<TestResource>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ForeignKeyTestDbContext(DbContextOptions<ForeignKeyTestDbContext> options)
        : TestableDbContext(options)
    {
        public DbSet<TestResource> TestResources => Set<TestResource>();

        protected override bool ShouldOverrideDeleteBehavior(IMutableForeignKey foreignKey)
        {
            return false;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TestResource>(entity =>
            {
                // @formatter:wrap_chained_method_calls chop_always

                entity.HasOne(resource => resource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull)
                    .WithOne(resource => resource.OptionalOneToOneForeignKeyAtRightSideOnDeleteClientSetNull)
                    .HasForeignKey<TestResource>("OneToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNullId")
                    .HasConstraintName("FK_OneToOptionalOne_ClientSetNull");

                entity.HasOne(resource => resource.OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull)
                    .WithOne(resource => resource.OptionalOneToOneForeignKeyAtRightSideOnDeleteSetNull)
                    .HasForeignKey<TestResource>("OneToOptionalOneForeignKeyAtLeftSideOnDeleteSetNullId")
                    .HasConstraintName("FK_OneToOptionalOne_SetNull")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(resource => resource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNull)
                    .WithMany(resource => resource.OptionalOneToManyForeignKeyAtRightSideOnDeleteClientSetNull)
                    .HasForeignKey("ManyToOptionalOneForeignKeyAtLeftSideOnDeleteClientSetNullId")
                    .HasConstraintName("FK_ManyToOptionalOne_ClientSetNull");

                entity.HasOne(resource => resource.ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNull)
                    .WithMany(resource => resource.OptionalOneToManyForeignKeyAtRightSideOnDeleteSetNull)
                    .HasForeignKey("ManyToOptionalOneForeignKeyAtLeftSideOnDeleteSetNullId")
                    .HasConstraintName("FK_ManyToOptionalOne_SetNull")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(resource => resource.OneToOneForeignKeyAtLeftSideOnDeleteCascade)
                    .WithOne(resource => resource.OneToOneForeignKeyAtRightSideOnDeleteCascade)
                    .HasForeignKey<TestResource>("OneToOneForeignKeyAtLeftSideOnDeleteCascadeId")
                    .HasConstraintName("FK_OneToOne_Cascade")
                    .IsRequired();

                entity.HasOne(resource => resource.ManyToOneForeignKeyAtLeftSideOnDeleteCascade)
                    .WithMany(resource => resource.OneToManyForeignKeyAtRightSideOnDeleteCascade)
                    .HasForeignKey("ManyToOneForeignKeyAtLeftSideOnDeleteCascadeId")
                    .HasConstraintName("FK_ManyToOne_Cascade")
                    .IsRequired();

                // @formatter:wrap_chained_method_calls restore
            });

            base.OnModelCreating(builder);
        }
    }
}
