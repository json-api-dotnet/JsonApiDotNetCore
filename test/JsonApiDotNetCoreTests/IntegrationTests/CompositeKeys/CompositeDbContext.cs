using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class CompositeDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<Dealership> Dealerships { get; set; }

        public CompositeDbContext(DbContextOptions<CompositeDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Car>()
                .HasKey(car => new
                {
                    car.RegionId,
                    car.LicensePlate
                });

            builder.Entity<Engine>()
                .HasOne(engine => engine.Car)
                .WithOne(car => car.Engine)
                .HasForeignKey<Engine>();

            builder.Entity<Dealership>()
                .HasMany(dealership => dealership.Inventory)
                .WithOne(car => car.Dealership);
        }
    }
}
