using Bogus;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

internal sealed class InjectionFakers
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Lazy<Faker<PostOffice>> _lazyPostOfficeFaker;
    private readonly Lazy<Faker<GiftCertificate>> _lazyGiftCertificateFaker;

    public Faker<PostOffice> PostOffice => _lazyPostOfficeFaker.Value;
    public Faker<GiftCertificate> GiftCertificate => _lazyGiftCertificateFaker.Value;

    public InjectionFakers(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;

        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        DateTime systemTimeUtc = timeProvider.GetUtcNow().UtcDateTime;

        _lazyPostOfficeFaker = new Lazy<Faker<PostOffice>>(() => new Faker<PostOffice>()
            .MakeDeterministic(systemTimeUtc)
            .CustomInstantiator(faker => new PostOffice(ResolveDbContext())
            {
                Address = faker.Address.FullAddress()
            }));

        _lazyGiftCertificateFaker = new Lazy<Faker<GiftCertificate>>(() => new Faker<GiftCertificate>()
            .MakeDeterministic(systemTimeUtc)
            .CustomInstantiator(_ => new GiftCertificate(ResolveDbContext()))
            .RuleFor(giftCertificate => giftCertificate.IssueDate, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds()));
    }

    private InjectionDbContext ResolveDbContext()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<InjectionDbContext>();
    }
}
