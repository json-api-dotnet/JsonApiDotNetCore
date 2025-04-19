using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

internal sealed class UniverseFakers
{
    private readonly Lazy<Faker<Constellation>> _lazyConstellationFaker = new(() => new Faker<Constellation>()
        .MakeDeterministic()
        .RuleFor(constellation => constellation.Name, faker => faker.Random.Word())
        .RuleFor(constellation => constellation.VisibleDuring, faker => faker.PickRandom<Season>()));

    private readonly Lazy<Faker<Star>> _lazyStarFaker = new(() => new Faker<Star>()
        .MakeDeterministic()
        .RuleFor(star => star.Name, faker => faker.Random.Word())
        .RuleFor(star => star.Kind, faker => faker.PickRandom<StarKind>())
        .RuleFor(star => star.SolarRadius, faker => faker.Random.Decimal(.01M, 1000M))
        .RuleFor(star => star.SolarMass, faker => faker.Random.Decimal(.001M, 100M))
        .RuleFor(star => star.IsVisibleFromEarth, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Planet>> _lazyPlanetFaker = new(() => new Faker<Planet>()
        .MakeDeterministic()
        .RuleFor(planet => planet.PublicName, faker => faker.Random.Word())
        .RuleFor(planet => planet.HasRingSystem, faker => faker.Random.Bool())
        .RuleFor(planet => planet.SolarMass, faker => faker.Random.Decimal(.001M, 100M)));

    private readonly Lazy<Faker<Moon>> _lazyMoonFaker = new(() => new Faker<Moon>()
        .MakeDeterministic()
        .RuleFor(moon => moon.Name, faker => faker.Random.Word())
        .RuleFor(moon => moon.SolarRadius, faker => faker.Random.Decimal(.01M, 1000M)));

    public Faker<Constellation> Constellation => _lazyConstellationFaker.Value;
    public Faker<Star> Star => _lazyStarFaker.Value;
    public Faker<Planet> Planet => _lazyPlanetFaker.Value;
    public Faker<Moon> Moon => _lazyMoonFaker.Value;
}
