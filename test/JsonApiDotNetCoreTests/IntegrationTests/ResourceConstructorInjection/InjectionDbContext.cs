using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class InjectionDbContext : TestableDbContext
{
    public TimeProvider TimeProvider { get; }

    public DbSet<PostOffice> PostOffices => Set<PostOffice>();
    public DbSet<GiftCertificate> GiftCertificates => Set<GiftCertificate>();


    public InjectionDbContext(DbContextOptions<InjectionDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        TimeProvider = timeProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<GiftCertificate>()
            .ComplexProperty(giftCertificate => giftCertificate.Recipient, complexBuilder => complexBuilder.ToJson());

        base.OnModelCreating(builder);
    }
}
