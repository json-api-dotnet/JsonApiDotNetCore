using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class InjectionDbContext : TestableDbContext
{
    public ISystemClock SystemClock { get; }

    public DbSet<PostOffice> PostOffices => Set<PostOffice>();
    public DbSet<GiftCertificate> GiftCertificates => Set<GiftCertificate>();

    public InjectionDbContext(DbContextOptions<InjectionDbContext> options, ISystemClock systemClock)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(systemClock);

        SystemClock = systemClock;
    }
}
