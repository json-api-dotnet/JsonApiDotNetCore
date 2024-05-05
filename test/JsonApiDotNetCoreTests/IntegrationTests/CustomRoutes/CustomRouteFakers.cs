using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

internal sealed class CustomRouteFakers
{
    private readonly Lazy<Faker<Town>> _lazyTownFaker = new(() => new Faker<Town>()
        .MakeDeterministic()
        .RuleFor(town => town.Name, faker => faker.Address.City())
        .RuleFor(town => town.Latitude, faker => faker.Address.Latitude())
        .RuleFor(town => town.Longitude, faker => faker.Address.Longitude()));

    private readonly Lazy<Faker<Civilian>> _lazyCivilianFaker = new(() => new Faker<Civilian>()
        .MakeDeterministic()
        .RuleFor(civilian => civilian.Name, faker => faker.Person.FullName));

    public Faker<Town> Town => _lazyTownFaker.Value;
    public Faker<Civilian> Civilian => _lazyCivilianFaker.Value;
}
