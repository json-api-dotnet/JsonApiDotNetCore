using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class ModelStateDbContext : DbContext
    {
        public DbSet<SystemDirectory> Directories { get; set; }
        public DbSet<SystemFile> Files { get; set; }

        public ModelStateDbContext(DbContextOptions<ModelStateDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SystemDirectory>()
                .HasMany(systemDirectory => systemDirectory.Subdirectories)
                .WithOne(x => x.Parent);

            builder.Entity<SystemDirectory>()
                .HasOne(systemDirectory => systemDirectory.Self)
                .WithOne();

            builder.Entity<SystemDirectory>()
                .HasOne(systemDirectory => systemDirectory.AlsoSelf)
                .WithOne();
        }
    }
}
