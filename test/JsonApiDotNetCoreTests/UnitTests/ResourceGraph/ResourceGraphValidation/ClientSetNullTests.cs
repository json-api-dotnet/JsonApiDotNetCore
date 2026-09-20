using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ResourceGraph.ResourceGraphValidation;

public sealed class ClientSetNullTests
{
    private static readonly JsonApiOptions WarnInsteadOfThrowOptions = new()
    {
        ThrowForProblematicEntityMappings = false
    };

    [Fact]
    public void Logs_warning_when_ClientSetNull_is_used_on_non_SqlServer_provider()
    {
        // Arrange
        IResourceGraph resourceGraph = CreateResourceGraph(typeof(Flight), typeof(Gate));

        using var dbContext = TestDbContextFactory.Create<ClientSetNullOnDeleteDbContext>(options => new ClientSetNullOnDeleteDbContext(options));

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
            "[WARNING] Relationship 'Flight.AssignedGate' uses DeleteBehavior.ClientSetNull, which causes the database foreign key to be created with NO ACTION. " +
            "When deleting a 'Gate' resource, JsonApiDotNetCore is forced to query and load referencing entities into memory so Entity Framework Core can set their " +
            "foreign keys to NULL. To delegate clearing foreign keys directly to the database engine and avoid executing extra database queries prior to deletion, " +
            "configure .OnDelete(DeleteBehavior.SetNull) in OnModelCreating.");
    }

    [Fact]
    public void Logs_no_warning_for_ClientSetNull_when_using_SqlServer_provider()
    {
        // Arrange
        IResourceGraph resourceGraph = CreateResourceGraph(typeof(Flight), typeof(Gate));

        using var dbContext = TestDbContextFactory.Create<ClientSetNullOnDeleteDbContext>(options => new ClientSetNullOnDeleteDbContext(options),
            options => options.ReplaceService<IDatabaseProvider, FakeSqlServerDatabaseProvider>());

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
    public void Logs_no_warning_for_ClientSetNull_when_using_SetNull()
    {
        // Arrange
        IResourceGraph resourceGraph = CreateResourceGraph(typeof(Flight), typeof(Gate));

        using var dbContext = TestDbContextFactory.Create<SetNullOnDeleteDbContext>(options => new SetNullOnDeleteDbContext(options));

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
    public void Logs_warning_for_ClientSetNull_on_non_public_entity_referencing_resource()
    {
        // Arrange
        IResourceGraph resourceGraph = CreateResourceGraph(typeof(Airplane));

        using var dbContext =
            TestDbContextFactory.Create<NonPublicEntityClientSetNullOnDeleteDbContext>(options => new NonPublicEntityClientSetNullOnDeleteDbContext(options));

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
            "[WARNING] Foreign key on 'AirplaneInspection' referencing resource 'Airplane' uses DeleteBehavior.ClientSetNull, which causes the database foreign key " +
            "to be created with NO ACTION. When deleting a 'Airplane' resource, JsonApiDotNetCore is forced to query and load referencing entities into memory " +
            "so Entity Framework Core can set their foreign keys to NULL. To delegate clearing foreign keys directly to the database engine and avoid executing " +
            "extra database queries prior to deletion, configure .OnDelete(DeleteBehavior.SetNull) in OnModelCreating.");
    }

    [Fact]
    public void Validates_multiple_models_when_building_resource_graph()
    {
        // Arrange
        var services = new ServiceCollection();

        using var loggerProvider = new CapturingLoggerProvider((category, logLevel) =>
            logLevel >= LogLevel.Warning && category.StartsWith("JsonApiDotNetCore.", StringComparison.Ordinal));

        // ReSharper disable once AccessToDisposedClosure
        services.AddLogging(builder => builder.AddProvider(loggerProvider));

        TestDbContextFactory.Add<ClientSetNullOnDeleteDbContext>(services);
        TestDbContextFactory.Add<NonPublicEntityClientSetNullOnDeleteDbContext>(services);

        services.AddJsonApi(options => options.ThrowForProblematicEntityMappings = false, dbContextTypes:
        [
            typeof(ClientSetNullOnDeleteDbContext),
            typeof(NonPublicEntityClientSetNullOnDeleteDbContext)
        ]);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        _ = serviceProvider.GetRequiredService<IResourceGraph>();

        // Assert
        IReadOnlyList<string> logLines = loggerProvider.GetLines();
        logLines.Should().HaveCount(2);
        logLines[0].Should().StartWith("[WARNING] Relationship 'Flight.AssignedGate' uses DeleteBehavior.ClientSetNull");
        logLines[1].Should().StartWith("[WARNING] Foreign key on 'AirplaneInspection' referencing resource 'Airplane' uses DeleteBehavior.ClientSetNull");
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

        TestDbContextFactory.Add<ClientSetNullOnDeleteDbContext>(services);
        services.AddJsonApi<ClientSetNullOnDeleteDbContext>();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Action action = () => _ = serviceProvider.GetRequiredService<IResourceGraph>();

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage(
            "Relationship 'Flight.AssignedGate' uses DeleteBehavior.ClientSetNull, which causes the database foreign key to be created with NO ACTION. " +
            "When deleting a 'Gate' resource, JsonApiDotNetCore is forced to query and load referencing entities into memory so Entity Framework Core can set their " +
            "foreign keys to NULL. To delegate clearing foreign keys directly to the database engine and avoid executing extra database queries prior to deletion, " +
            "configure .OnDelete(DeleteBehavior.SetNull) in OnModelCreating." + Environment.NewLine + Environment.NewLine +
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
    public sealed class Flight : Identifiable<long>
    {
        [Attr]
        [Required]
        public int? PassengerCount { get; set; }

        [HasOne]
        public Gate? AssignedGate { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Gate : Identifiable<long>
    {
        [Attr]
        public string GateCode { get; set; } = null!;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Airplane : Identifiable<long>
    {
        [Attr]
        [Required]
        public int? PassengerCapacity { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [NoResource]
    public sealed class AirplaneInspection
    {
        public long Id { get; set; }
        public string? Inspector { get; set; }
        public long? AirplaneId { get; set; }
        public Airplane? Airplane { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ClientSetNullOnDeleteDbContext(DbContextOptions<ClientSetNullOnDeleteDbContext> options)
        : DbContext(options)
    {
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<Gate> Gates => Set<Gate>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // @formatter:wrap_chained_method_calls chop_always

            builder.Entity<Flight>()
                .HasOne(flight => flight.AssignedGate)
                .WithOne()
                .HasForeignKey<Flight>("AssignedGateId")
                .OnDelete(DeleteBehavior.ClientSetNull);

            // @formatter:wrap_chained_method_calls restore
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class SetNullOnDeleteDbContext(DbContextOptions<SetNullOnDeleteDbContext> options)
        : DbContext(options)
    {
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<Gate> Gates => Set<Gate>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // @formatter:wrap_chained_method_calls chop_always

            builder.Entity<Flight>()
                .HasOne(flight => flight.AssignedGate)
                .WithOne()
                .HasForeignKey<Flight>("AssignedGateId")
                .OnDelete(DeleteBehavior.SetNull);

            // @formatter:wrap_chained_method_calls restore
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class NonPublicEntityClientSetNullOnDeleteDbContext(DbContextOptions<NonPublicEntityClientSetNullOnDeleteDbContext> options)
        : DbContext(options)
    {
        public DbSet<AirplaneInspection> AirplaneInspections => Set<AirplaneInspection>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // @formatter:wrap_chained_method_calls chop_always

            builder.Entity<AirplaneInspection>()
                .HasOne(inspection => inspection.Airplane)
                .WithMany()
                .HasForeignKey(inspection => inspection.AirplaneId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // @formatter:wrap_chained_method_calls restore
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    private sealed class FakeSqlServerDatabaseProvider : IDatabaseProvider
    {
        public string Name => "Microsoft.EntityFrameworkCore.SqlServer";

        public bool IsConfigured(IDbContextOptions options)
        {
            return true;
        }
    }
}
