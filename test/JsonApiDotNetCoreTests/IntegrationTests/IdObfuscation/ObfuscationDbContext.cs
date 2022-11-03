using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ObfuscationDbContext : TestableDbContext
{
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<DebitCard> DebitCards => Set<DebitCard>();

    public ObfuscationDbContext(DbContextOptions<ObfuscationDbContext> options)
        : base(options)
    {
    }
}
