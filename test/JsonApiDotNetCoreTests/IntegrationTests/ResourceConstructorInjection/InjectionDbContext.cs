using JetBrains.Annotations;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class InjectionDbContext : DbContext
    {
        public ISystemClock SystemClock { get; }

        public DbSet<PostOffice> PostOffice => Set<PostOffice>();
        public DbSet<GiftCertificate> GiftCertificates => Set<GiftCertificate>();

        public InjectionDbContext(DbContextOptions<InjectionDbContext> options, ISystemClock systemClock)
            : base(options)
        {
            ArgumentGuard.NotNull(systemClock, nameof(systemClock));

            SystemClock = systemClock;
        }
    }
}
