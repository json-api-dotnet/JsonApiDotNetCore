using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class EagerLoadingDbContext : DbContext
{
    public DbSet<State> States => Set<State>();
    public DbSet<Street> Streets => Set<Street>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Door> Doors => Set<Door>();

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
            // The PrimaryDoor relationship property is declared as nullable, because the Door type is not publicly exposed,
            // so we don't want ModelState validation to fail when it isn't provided by the client. But because
            // BuildingRepository ensures a value is assigned on Create, we can make it a required relationship in the database.
            .IsRequired();

        builder.Entity<Building>()
            .HasOne(building => building.SecondaryDoor)
            .WithOne()
            .HasForeignKey<Building>("SecondaryDoorId");
    }
}
