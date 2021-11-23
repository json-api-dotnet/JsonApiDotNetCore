using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ObfuscationDbContext : DbContext
    {
        public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
        public DbSet<DebitCard> DebitCards => Set<DebitCard>();

        public ObfuscationDbContext(DbContextOptions<ObfuscationDbContext> options)
            : base(options)
        {
        }
    }
}
