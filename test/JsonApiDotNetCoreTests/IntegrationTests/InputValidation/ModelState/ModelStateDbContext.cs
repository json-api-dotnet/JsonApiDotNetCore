using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ModelStateDbContext : DbContext
{
    public DbSet<SystemVolume> Volumes => Set<SystemVolume>();
    public DbSet<SystemDirectory> Directories => Set<SystemDirectory>();
    public DbSet<SystemFile> Files => Set<SystemFile>();

    public ModelStateDbContext(DbContextOptions<ModelStateDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<SystemVolume>()
            .HasOne(systemVolume => systemVolume.RootDirectory)
            .WithOne()
            .HasForeignKey<SystemVolume>("RootDirectoryId")
            .IsRequired();

        builder.Entity<SystemDirectory>()
            .HasMany(systemDirectory => systemDirectory.Subdirectories)
            .WithOne(systemDirectory => systemDirectory.Parent!);

        builder.Entity<SystemDirectory>()
            .HasOne(systemDirectory => systemDirectory.Self)
            .WithOne();

        builder.Entity<SystemDirectory>()
            .HasOne(systemDirectory => systemDirectory.AlsoSelf)
            .WithOne();
    }
}
