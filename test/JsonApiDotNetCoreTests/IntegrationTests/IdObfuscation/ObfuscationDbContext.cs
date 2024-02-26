using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ObfuscationDbContext(DbContextOptions<ObfuscationDbContext> options) : TestableDbContext(options)
{
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<DebitCard> DebitCards => Set<DebitCard>();
}
