using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class SerializationDbContext(DbContextOptions<SerializationDbContext> options) : TestableDbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Scholarship> Scholarships => Set<Scholarship>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Scholarship>()
            .HasMany(scholarship => scholarship.Participants)
            .WithOne(student => student.Scholarship!);

        base.OnModelCreating(builder);
    }
}
