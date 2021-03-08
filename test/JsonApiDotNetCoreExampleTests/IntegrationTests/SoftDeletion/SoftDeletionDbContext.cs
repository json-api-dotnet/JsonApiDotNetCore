using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SoftDeletionDbContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }

        public SoftDeletionDbContext(DbContextOptions<SoftDeletionDbContext> options)
            : base(options)
        {
        }
    }
}
