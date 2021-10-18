using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SerializationDbContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Scholarship> Scholarships => Set<Scholarship>();

        public SerializationDbContext(DbContextOptions<SerializationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Scholarship>()
                .HasMany(scholarship => scholarship.Participants)
                .WithOne(student => student.Scholarship);
        }
    }
}
