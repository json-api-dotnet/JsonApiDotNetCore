using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph.ResourceGraphValidation;

public sealed class IdentifyingForeignKeyTests
{
    private static readonly JsonApiOptions WarnInsteadOfThrowOptions = new()
    {
        ThrowForProblematicEntityMappings = false
    };

    [Fact]
    public void Logs_warning_when_identifying_foreign_key_is_used()
    {
        // Arrange
        IResourceGraph resourceGraph = CreateResourceGraph(typeof(Battery), typeof(Flashlight));

        using var dbContext = TestDbContextFactory.Create<IdentifyingForeignKeyDbContext>(options => new IdentifyingForeignKeyDbContext(options));

        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        ILogger<EntityFrameworkCoreModelValidator> logger = loggerFactory.CreateLogger<EntityFrameworkCoreModelValidator>();

        var validator = new EntityFrameworkCoreModelValidator([dbContext], WarnInsteadOfThrowOptions, resourceGraph, logger);

        // Act
        validator.Validate();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);

        logLines[0].Should().Be(
            "[WARNING] Relationship 'Battery.Flashlight' uses an identifying foreign key where 'Battery.Id' is both the primary key and the foreign key to 'Flashlight'. " +
            "In JSON:API, resources must have independent identities. Using an identifying foreign key prevents reassigning the relationship to another " +
            "resource (because primary keys cannot be updated) and prevents clearing the relationship. To fix this, define a dedicated foreign key column " +
            "instead, for example: builder.Entity<Battery>().HasOne(...).WithOne(...).HasForeignKey<Battery>(\"FlashlightId\"). Note that doing so results in database changes.");
    }

    [Fact]
    public void Logs_no_warning_when_foreign_key_has_dedicated_column()
    {
        // Arrange
        IResourceGraph resourceGraph = CreateResourceGraph(typeof(Battery), typeof(Flashlight));

        using var dbContext = TestDbContextFactory.Create<DedicatedColumnForeignKeyDbContext>(options => new DedicatedColumnForeignKeyDbContext(options));

        using var loggerProvider = new CapturingLoggerProvider(LogLevel.Warning);
        using var loggerFactory = new LoggerFactory([loggerProvider]);
        ILogger<EntityFrameworkCoreModelValidator> logger = loggerFactory.CreateLogger<EntityFrameworkCoreModelValidator>();

        var validator = new EntityFrameworkCoreModelValidator([dbContext], WarnInsteadOfThrowOptions, resourceGraph, logger);

        // Act
        validator.Validate();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().BeEmpty();
    }

    [Fact]
    public void Validates_model_when_building_resource_graph()
    {
        // Arrange
        var services = new ServiceCollection();

        using var loggerProvider = new CapturingLoggerProvider((category, logLevel) =>
            logLevel >= LogLevel.Warning && category.StartsWith("JsonApiDotNetCore.", StringComparison.Ordinal));

        // ReSharper disable once AccessToDisposedClosure
        services.AddLogging(builder => builder.AddProvider(loggerProvider));

        TestDbContextFactory.Add<IdentifyingForeignKeyDbContext>(services);
        services.AddJsonApi<IdentifyingForeignKeyDbContext>(options => options.ThrowForProblematicEntityMappings = false);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        _ = serviceProvider.GetRequiredService<IResourceGraph>();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(1);
        logLines[0].Should().StartWith("[WARNING] Relationship 'Battery.Flashlight' uses an identifying foreign key");
    }

    [Fact]
    public void Throws_when_building_resource_graph()
    {
        // Arrange
        var services = new ServiceCollection();

        using var loggerProvider = new CapturingLoggerProvider((category, logLevel) =>
            logLevel >= LogLevel.Warning && category.StartsWith("JsonApiDotNetCore.", StringComparison.Ordinal));

        // ReSharper disable once AccessToDisposedClosure
        services.AddLogging(builder => builder.AddProvider(loggerProvider));

        TestDbContextFactory.Add<IdentifyingForeignKeyDbContext>(services);
        services.AddJsonApi<IdentifyingForeignKeyDbContext>();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Action action = () => _ = serviceProvider.GetRequiredService<IResourceGraph>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            "Relationship 'Battery.Flashlight' uses an identifying foreign key where 'Battery.Id' is both the primary key and the foreign key to 'Flashlight'. " +
            "In JSON:API, resources must have independent identities. Using an identifying foreign key prevents reassigning the relationship to another " +
            "resource (because primary keys cannot be updated) and prevents clearing the relationship. To fix this, define a dedicated foreign key column " +
            "instead, for example: builder.Entity<Battery>().HasOne(...).WithOne(...).HasForeignKey<Battery>(\"FlashlightId\"). Note that doing so results in database changes." +
            Environment.NewLine + Environment.NewLine +
            "To log a warning instead of throwing this exception, set options.ThrowForProblematicEntityMappings to false.");
    }

    private static IResourceGraph CreateResourceGraph(params Type[] resourceTypes)
    {
        var options = new JsonApiOptions();
        var builder = new ResourceGraphBuilder(options, NullLoggerFactory.Instance);

        foreach (Type resourceType in resourceTypes)
        {
            builder.Add(resourceType);
        }

        return builder.Build();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Battery : Identifiable<long>
    {
        [Attr]
        public byte? PercentageCharged { get; set; }

        [HasOne]
        public Flashlight? Flashlight { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Flashlight : Identifiable<long>
    {
        [Attr]
        public short? WeightInGrams { get; set; }

        [HasOne]
        public Battery? Battery { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class IdentifyingForeignKeyDbContext(DbContextOptions<IdentifyingForeignKeyDbContext> options)
        : DbContext(options)
    {
        public DbSet<Battery> Batteries => Set<Battery>();
        public DbSet<Flashlight> Flashlights => Set<Flashlight>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // @formatter:wrap_chained_method_calls chop_always

            builder.Entity<Battery>()
                .HasOne(battery => battery.Flashlight)
                .WithOne(flashlight => flashlight.Battery)
                .HasForeignKey<Battery>();

            // @formatter:wrap_chained_method_calls restore
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class DedicatedColumnForeignKeyDbContext(DbContextOptions<DedicatedColumnForeignKeyDbContext> options)
        : DbContext(options)
    {
        public DbSet<Battery> Batteries => Set<Battery>();
        public DbSet<Flashlight> Flashlights => Set<Flashlight>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // @formatter:wrap_chained_method_calls chop_always

            builder.Entity<Battery>()
                .HasOne(battery => battery.Flashlight)
                .WithOne(flashlight => flashlight.Battery)
                .HasForeignKey<Battery>("FlashlightId")
                .OnDelete(DeleteBehavior.SetNull);

            // @formatter:wrap_chained_method_calls restore
        }
    }
}
