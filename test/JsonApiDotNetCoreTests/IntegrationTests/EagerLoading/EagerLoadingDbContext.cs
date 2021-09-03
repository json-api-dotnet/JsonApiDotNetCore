using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
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
                .HasForeignKey<Building>("PrimaryDoorId")
                .IsRequired();

            builder.Entity<Building>()
                .HasOne(building => building.SecondaryDoor)
                .WithOne()
                .HasForeignKey<Building>("SecondaryDoorId")
                .IsRequired(false);
        }
    }
}
