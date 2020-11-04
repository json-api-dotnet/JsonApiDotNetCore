using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class CompositeDbContext : DbContext
    {
        public DbSet<Car> Cars { get; set; }

        public DbSet<Engine> Engines { get; set; }

        public DbSet<Journey> Journeys { get; set; }

        public CompositeDbContext(DbContextOptions<CompositeDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .HasKey(c => new {c.RegionId, c.LicensePlate});

            modelBuilder.Entity<Engine>()
                .HasOne(e => e.Car)
                .WithOne(c => c.Engine)
                .HasForeignKey<Engine>();

            modelBuilder.Entity<Journey>()
                .HasOne(e => e.Car)
                .WithMany(c => c.Journeys);
        }
    }
}
