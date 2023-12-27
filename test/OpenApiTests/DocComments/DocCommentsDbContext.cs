using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace OpenApiTests.DocComments;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class DocCommentsDbContext(DbContextOptions<DocCommentsDbContext> options) : TestableDbContext(options)
{
    public DbSet<Skyscraper> Skyscrapers => Set<Skyscraper>();
    public DbSet<Elevator> Elevators => Set<Elevator>();
    public DbSet<Space> Spaces => Set<Space>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Skyscraper>()
            .HasOne(skyscraper => skyscraper.Elevator)
            .WithOne(elevator => elevator.ExistsIn)
            .HasForeignKey<Skyscraper>("ElevatorId");

        base.OnModelCreating(builder);
    }
}
