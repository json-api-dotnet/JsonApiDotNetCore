using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class IssueDbContext : DbContext
    {
        public DbSet<Engagement> Engagements { get; set; }
        public DbSet<EngagementParty> EngagementParties { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }

        public IssueDbContext(DbContextOptions<IssueDbContext> options)
            : base(options)
        {
        }
    }
}
