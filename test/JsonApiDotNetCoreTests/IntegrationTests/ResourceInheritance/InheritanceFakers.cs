using System;
using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    internal sealed class InheritanceFakers : FakerContainer
    {
        private readonly Lazy<Faker<Man>> _lazyManFaker = new(() =>
            new Faker<Man>()
                .UseSeed(GetFakerSeed())
                .RuleFor(man => man.FamilyName, faker => faker.Person.LastName)
                .RuleFor(man => man.IsRetired, faker => faker.Random.Bool())
                .RuleFor(man => man.HasBeard, faker => faker.Random.Bool()));

        private readonly Lazy<Faker<Woman>> _lazyWomanFaker = new(() =>
            new Faker<Woman>()
                .UseSeed(GetFakerSeed())
                .RuleFor(woman => woman.FamilyName, faker => faker.Person.LastName)
                .RuleFor(woman => woman.IsRetired, faker => faker.Random.Bool())
                .RuleFor(woman => woman.IsPregnant, faker => faker.Random.Bool()));

        private readonly Lazy<Faker<Book>> _lazyBookFaker = new(() =>
            new Faker<Book>()
                .UseSeed(GetFakerSeed())
                .RuleFor(book => book.Title, faker => faker.Commerce.ProductName())
                .RuleFor(book => book.PageCount, faker => faker.Random.Int(50, 150)));

        private readonly Lazy<Faker<Video>> _lazyVideoFaker = new(() =>
            new Faker<Video>()
                .UseSeed(GetFakerSeed())
                .RuleFor(video => video.Title, faker => faker.Commerce.ProductName())
                .RuleFor(video => video.DurationInSeconds, faker => faker.Random.Int(250, 750)));

        private readonly Lazy<Faker<FamilyHealthInsurance>> _lazyFamilyHealthInsuranceFaker = new(() =>
            new Faker<FamilyHealthInsurance>()
                .UseSeed(GetFakerSeed())
                .RuleFor(familyHealthInsurance => familyHealthInsurance.HasMonthlyFee, faker => faker.Random.Bool())
                .RuleFor(familyHealthInsurance => familyHealthInsurance.PermittedFamilySize, faker => faker.Random.Int(2, 10)));

        private readonly Lazy<Faker<CompanyHealthInsurance>> _lazyCompanyHealthInsuranceFaker = new(() =>
            new Faker<CompanyHealthInsurance>()
                .UseSeed(GetFakerSeed())
                .RuleFor(companyHealthInsurance => companyHealthInsurance.HasMonthlyFee, faker => faker.Random.Bool())
                .RuleFor(companyHealthInsurance => companyHealthInsurance.CompanyCode, faker => faker.Company.CompanyName()));

        public Faker<Man> Man => _lazyManFaker.Value;
        public Faker<Woman> Woman => _lazyWomanFaker.Value;
        public Faker<Book> Book => _lazyBookFaker.Value;
        public Faker<Video> Video => _lazyVideoFaker.Value;
        public Faker<FamilyHealthInsurance> FamilyHealthInsurance => _lazyFamilyHealthInsuranceFaker.Value;
        public Faker<CompanyHealthInsurance> CompanyHealthInsurance => _lazyCompanyHealthInsuranceFaker.Value;
    }
}
