using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class HeaderFakers : FakerContainer
{
    private readonly Lazy<Faker<Country>> _lazyCountryFaker = new(() => new Faker<Country>()
        .UseSeed(GetFakerSeed())
        .RuleFor(country => country.Name, faker => faker.Address.Country())
        .RuleFor(country => country.Population, faker => faker.Random.Long(0, 2_000_000_000)));

    public Faker<Country> Country => _lazyCountryFaker.Value;
}
