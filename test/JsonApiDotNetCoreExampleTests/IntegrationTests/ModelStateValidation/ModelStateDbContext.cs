using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ModelStateDbContext : DbContext
    {
        public DbSet<SystemDirectory> Directories { get; set; }
        public DbSet<SystemFile> Files { get; set; }

        public ModelStateDbContext(DbContextOptions<ModelStateDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SystemDirectory>()
                .HasMany(systemDirectory => systemDirectory.Subdirectories)
                .WithOne(systemDirectory => systemDirectory.Parent);

            builder.Entity<SystemDirectory>()
                .HasOne(systemDirectory => systemDirectory.Self)
                .WithOne();

            builder.Entity<SystemDirectory>()
                .HasOne(systemDirectory => systemDirectory.AlsoSelf)
                .WithOne();
        }
    }
}
