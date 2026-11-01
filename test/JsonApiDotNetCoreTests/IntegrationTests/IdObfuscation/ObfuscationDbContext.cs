using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ObfuscationDbContext(DbContextOptions<ObfuscationDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<DebitCard> DebitCards => Set<DebitCard>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DebitCard>()
            .ComplexProperty(card => card.Limits, complexBuilder => complexBuilder.ToJson());

        builder.Entity<DebitCard>()
            .ComplexCollection(card => card.Notes, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
