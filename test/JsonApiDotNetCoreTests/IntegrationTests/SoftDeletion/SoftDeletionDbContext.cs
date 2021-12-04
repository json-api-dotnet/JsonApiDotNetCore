using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class SoftDeletionDbContext : DbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();

    public SoftDeletionDbContext(DbContextOptions<SoftDeletionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Company>()
            .HasQueryFilter(company => company.SoftDeletedAt == null);

        builder.Entity<Department>()
            .HasQueryFilter(department => department.SoftDeletedAt == null);
    }
}
