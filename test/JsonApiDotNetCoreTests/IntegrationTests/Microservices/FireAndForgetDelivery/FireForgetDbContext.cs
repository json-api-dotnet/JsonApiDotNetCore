#nullable disable

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FireForgetDbContext : DbContext
    {
        public DbSet<DomainUser> Users { get; set; }
        public DbSet<DomainGroup> Groups { get; set; }

        public FireForgetDbContext(DbContextOptions<FireForgetDbContext> options)
            : base(options)
        {
        }
    }
}
