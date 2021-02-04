using System;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

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
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _lazyPostOfficeFaker = new Lazy<Faker<PostOffice>>(() =>
                new Faker<PostOffice>()
                    .UseSeed(GetFakerSeed())
                    .CustomInstantiator(f => new PostOffice(ResolveDbContext()))
                    .RuleFor(postOffice => postOffice.Address, f => f.Address.FullAddress()));

            _lazyGiftCertificateFaker = new Lazy<Faker<GiftCertificate>>(() =>
                new Faker<GiftCertificate>()
                    .UseSeed(GetFakerSeed())
                    .CustomInstantiator(f => new GiftCertificate(ResolveDbContext()))
                    .RuleFor(giftCertificate => giftCertificate.IssueDate, f => f.Date.PastOffset()));
        }

        private InjectionDbContext ResolveDbContext()
        {
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<InjectionDbContext>();
        }
    }
}
