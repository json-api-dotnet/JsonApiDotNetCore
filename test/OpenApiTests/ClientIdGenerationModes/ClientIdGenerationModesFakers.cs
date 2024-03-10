using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.ClientIdGenerationModes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ClientIdGenerationModesFakers : FakerContainer
{
    private readonly Lazy<Faker<Player>> _lazyPlayerFaker = new(() => new Faker<Player>()
        .UseSeed(GetFakerSeed())
        .RuleFor(player => player.UserName, faker => faker.Person.UserName));

    private readonly Lazy<Faker<Game>> _lazyGameFaker = new(() => new Faker<Game>()
        .UseSeed(GetFakerSeed())
        .RuleFor(game => game.Title, faker => faker.Commerce.ProductName())
        .RuleFor(game => game.PurchasePrice, faker => faker.Finance.Amount(1, 80)));

    private readonly Lazy<Faker<PlayerGroup>> _lazyGroupFaker = new(() => new Faker<PlayerGroup>()
        .UseSeed(GetFakerSeed())
        .RuleFor(playerGroup => playerGroup.Name, faker => faker.Person.Company.Name));

    public Faker<Player> Player => _lazyPlayerFaker.Value;
    public Faker<Game> Game => _lazyGameFaker.Value;
    public Faker<PlayerGroup> Group => _lazyGroupFaker.Value;
}
