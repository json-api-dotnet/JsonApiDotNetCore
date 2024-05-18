using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

internal sealed class CompositeKeyFakers
{
    private readonly Lazy<Faker<Car>> _lazyCarFaker = new(() => new Faker<Car>()
        .MakeDeterministic()
        .RuleFor(car => car.LicensePlate, faker => faker.Random.Replace("??-??-##"))
        .RuleFor(car => car.RegionId, faker => faker.Random.Long(100, 999)));

    private readonly Lazy<Faker<Engine>> _lazyEngineFaker = new(() => new Faker<Engine>()
        .MakeDeterministic()
        .RuleFor(engine => engine.SerialCode, faker => faker.Random.Replace("????-????")));

    private readonly Lazy<Faker<Dealership>> _lazyDealershipFaker = new(() => new Faker<Dealership>()
        .MakeDeterministic()
        .RuleFor(dealership => dealership.Address, faker => faker.Address.FullAddress()));

    public Faker<Car> Car => _lazyCarFaker.Value;
    public Faker<Engine> Engine => _lazyEngineFaker.Value;
    public Faker<Dealership> Dealership => _lazyDealershipFaker.Value;
}
