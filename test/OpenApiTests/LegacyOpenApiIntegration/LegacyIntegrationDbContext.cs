using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class LegacyIntegrationDbContext : DbContext
    {
        public DbSet<Airplane> Airplanes => Set<Airplane>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<FlightAttendant> FlightAttendants => Set<FlightAttendant>();

        public LegacyIntegrationDbContext(DbContextOptions<LegacyIntegrationDbContext> options)
            : base(options)
        {
        }

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
        }
    }
}
