using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class IssueDbContext : DbContext
    {
        public DbSet<Engagement> Engagements { get; set; }
        public DbSet<EngagementParty> EngagementParties { get; set; }

        public IssueDbContext(DbContextOptions<IssueDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EngagementParty>()
                .HasOne(engagementParty => engagementParty.Engagement)
                .WithMany(engagement => engagement.Parties)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
