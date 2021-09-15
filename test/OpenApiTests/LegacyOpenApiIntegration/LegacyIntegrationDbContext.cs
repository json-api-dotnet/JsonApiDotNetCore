using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace OpenApiTests.LegacyOpenApiIntegration
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class LegacyIntegrationDbContext : DbContext
    {
        public DbSet<Airplane> Airplanes { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightAttendant> FlightAttendants { get; set; }

        public LegacyIntegrationDbContext(DbContextOptions<LegacyIntegrationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Flight>()
                .HasMany(flight => flight.CabinPersonnel)
                .WithMany(flightAttendant => flightAttendant.ScheduledForFlights);

            builder.Entity<Flight>()
                .HasMany(flight => flight.BackupPersonnel)
                .WithMany(flightAttendant => flightAttendant.StandbyForFlights);
        }
    }
}
