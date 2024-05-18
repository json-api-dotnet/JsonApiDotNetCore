using Bogus;
using JsonApiDotNetCore;
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
        ArgumentGuard.NotNull(serviceProvider);

        _serviceProvider = serviceProvider;

        _lazyPostOfficeFaker = new Lazy<Faker<PostOffice>>(() => new Faker<PostOffice>()
            .MakeDeterministic()
            .CustomInstantiator(_ => new PostOffice(ResolveDbContext()))
            .RuleFor(postOffice => postOffice.Address, faker => faker.Address.FullAddress()));

        _lazyGiftCertificateFaker = new Lazy<Faker<GiftCertificate>>(() => new Faker<GiftCertificate>()
            .MakeDeterministic()
            .CustomInstantiator(_ => new GiftCertificate(ResolveDbContext()))
            .RuleFor(giftCertificate => giftCertificate.IssueDate, faker => faker.Date.PastOffset().TruncateToWholeMilliseconds()));
    }

    private InjectionDbContext ResolveDbContext()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<InjectionDbContext>();
    }
}
