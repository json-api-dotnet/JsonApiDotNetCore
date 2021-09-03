using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SerializationDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Scholarship> Scholarships { get; set; }

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
