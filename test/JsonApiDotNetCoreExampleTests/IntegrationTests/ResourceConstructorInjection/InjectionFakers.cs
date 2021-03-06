using System;
using Bogus;
using JsonApiDotNetCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceConstructorInjection
{
    internal sealed class InjectionFakers : FakerContainer
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Lazy<Faker<PostOffice>> _lazyPostOfficeFaker;
        private readonly Lazy<Faker<GiftCertificate>> _lazyGiftCertificateFaker;

        public Faker<PostOffice> PostOffice => _lazyPostOfficeFaker.Value;
        public Faker<GiftCertificate> GiftCertificate => _lazyGiftCertificateFaker.Value;

        public InjectionFakers(IServiceProvider serviceProvider)
        {
            ArgumentGuard.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;

            _lazyPostOfficeFaker = new Lazy<Faker<PostOffice>>(() =>
                new Faker<PostOffice>()
                    .UseSeed(GetFakerSeed())
                    .CustomInstantiator(_ => new PostOffice(ResolveDbContext()))
                    .RuleFor(postOffice => postOffice.Address, faker => faker.Address.FullAddress()));

            _lazyGiftCertificateFaker = new Lazy<Faker<GiftCertificate>>(() =>
                new Faker<GiftCertificate>()
                    .UseSeed(GetFakerSeed())
                    .CustomInstantiator(_ => new GiftCertificate(ResolveDbContext()))
                    .RuleFor(giftCertificate => giftCertificate.IssueDate, faker => faker.Date.PastOffset()));
        }

        private InjectionDbContext ResolveDbContext()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<InjectionDbContext>();
        }
    }
}
