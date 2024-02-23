using System.Globalization;
using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.Headers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class HeaderFakers : FakerContainer
{
    private static readonly Lazy<string[]> LazyLanguageNames = new(() => CultureInfo
        .GetCultures(CultureTypes.NeutralCultures)
        .Select(culture => culture.DisplayName)
        .ToArray());

    private static readonly Lazy<string[]> LazyLanguageCodes = new(() => CultureInfo
        .GetCultures(CultureTypes.NeutralCultures)
        .Select(culture => culture.ThreeLetterISOLanguageName)
        .ToArray());

    private readonly Lazy<Faker<Country>> _lazyCountryFaker = new(() => new Faker<Country>()
        .UseSeed(GetFakerSeed())
        .RuleFor(country => country.Name, faker => faker.Address.Country())
        .RuleFor(country => country.Population, faker => faker.Random.Long(0, 2_000_000_000)));

    private readonly Lazy<Faker<Language>> _lazyLanguageFaker = new(() => new Faker<Language>()
        .UseSeed(GetFakerSeed())
        .RuleFor(language => language.Name, faker => faker.PickRandom(LazyLanguageNames.Value))
        .RuleFor(language => language.Code, faker => faker.PickRandom(LazyLanguageCodes.Value)));

    public Faker<Country> Country => _lazyCountryFaker.Value;
    public Faker<Language> Language => _lazyLanguageFaker.Value;
}
