using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ObfuscationDbContext : DbContext
    {
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<DebitCard> DebitCards { get; set; }

        public ObfuscationDbContext(DbContextOptions<ObfuscationDbContext> options)
            : base(options)
        {
        }
    }
}
