using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class EagerLoadingDbContext : DbContext
    {
        public DbSet<State> States { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<Building> Buildings { get; set; }

        public EagerLoadingDbContext(DbContextOptions<EagerLoadingDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Building>()
                .HasOne(building => building.PrimaryDoor)
                .WithOne()
                .HasForeignKey<Building>("PrimaryDoorKey")
                .IsRequired();

            builder.Entity<Building>()
                .HasOne(building => building.SecondaryDoor)
                .WithOne()
                .HasForeignKey<Building>("SecondaryDoorKey")
                .IsRequired(false);
        }
    }
}
