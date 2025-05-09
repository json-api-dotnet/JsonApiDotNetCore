using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompoundAttributeDbContext(DbContextOptions<CompoundAttributeDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<CloudAccount> Accounts => Set<CloudAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CloudAccount>()
            .OwnsOne(account => account.EmergencyContact)
            .ToJson();

        builder.Entity<CloudAccount>()
            .OwnsOne(account => account.BackupEmergencyContact)
            .ToJson();

        builder.Entity<CloudAccount>()
            .OwnsMany(account => account.Contacts)
            .ToJson();

        base.OnModelCreating(builder);
    }
}
