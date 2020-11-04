using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class CompositeDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<Dealership> Dealerships { get; set; }

        public CompositeDbContext(DbContextOptions<CompositeDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .HasKey(car => new {car.RegionId, car.LicensePlate});

            modelBuilder.Entity<Engine>()
                .HasOne(engine => engine.Car)
                .WithOne(car => car.Engine)
                .HasForeignKey<Engine>();

            modelBuilder.Entity<Dealership>()
                .HasMany(dealership => dealership.Inventory)
                .WithOne(car => car.Dealership);
        }
    }
}
