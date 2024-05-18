using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

internal sealed class ZeroKeyFakers
{
    private readonly Lazy<Faker<Game>> _lazyGameFaker = new(() => new Faker<Game>()
        .MakeDeterministic()
        .RuleFor(game => game.Title, faker => faker.Random.Words()));

    private readonly Lazy<Faker<Player>> _lazyPlayerFaker = new(() => new Faker<Player>()
        .MakeDeterministic()
        .RuleFor(player => player.Id, faker => faker.Person.UserName)
        .RuleFor(player => player.EmailAddress, faker => faker.Person.Email));

    private readonly Lazy<Faker<Map>> _lazyMapFaker = new(() => new Faker<Map>()
        .MakeDeterministic()
        .RuleFor(map => map.Id, faker => faker.Random.Guid())
        .RuleFor(map => map.Name, faker => faker.Random.Words()));

    public Faker<Game> Game => _lazyGameFaker.Value;
    public Faker<Player> Player => _lazyPlayerFaker.Value;
    public Faker<Map> Map => _lazyMapFaker.Value;
}
