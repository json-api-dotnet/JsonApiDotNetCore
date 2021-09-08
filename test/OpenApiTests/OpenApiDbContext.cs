using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OpenApiDbContext : DbContext
    {
        public DbSet<Airplane> Airplanes { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightAttendant> FlightAttendants { get; set; }

        public OpenApiDbContext(DbContextOptions<OpenApiDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Flight>()
                .HasMany(flight => flight.CabinPersonnel)
                .WithMany(flightAttendant => flightAttendant.Flights);

            builder.Entity<FlightAttendant>()
                .Ignore(flightAttendant => flightAttendant.DestinationPreferences);

            builder.Entity<Flight>()
                .HasOne(flight => flight.Purser)
                .WithOne(flightAttendant => flightAttendant.PurserOnFlight)
                .HasForeignKey<Flight>("PurserId");
        }
    }
}
