using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace OpenApiTests.LegacyOpenApiIntegration;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LegacyIntegrationDbContext(DbContextOptions<LegacyIntegrationDbContext> options) : TestableDbContext(options)
{
    public DbSet<Airplane> Airplanes => Set<Airplane>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<FlightAttendant> FlightAttendants => Set<FlightAttendant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Flight>()
            .HasMany(flight => flight.CabinCrewMembers)
            .WithMany(flightAttendant => flightAttendant.ScheduledForFlights);

        builder.Entity<Flight>()
            .HasOne(flight => flight.Purser)
            .WithMany(flightAttendant => flightAttendant.PurserOnFlights);

        builder.Entity<Flight>()
            .HasOne(flight => flight.BackupPurser)
            .WithMany();

        base.OnModelCreating(builder);
    }
}
