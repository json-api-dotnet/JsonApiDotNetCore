using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.ClientGeneratedId;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ClientGeneratedIdFakers : FakerContainer
{
    private readonly Lazy<Faker<Player>> _lazyPlayerFaker = new(() => new Faker<Player>()
        .UseSeed(GetFakerSeed())
        .RuleFor(player => player.Name, faker => faker.Person.UserName));

    private readonly Lazy<Faker<Game>> _lazyGameFaker = new(() => new Faker<Game>()
        .UseSeed(GetFakerSeed())
        .RuleFor(player => player.Name, faker => faker.Commerce.ProductName())
        .RuleFor(player => player.Price, faker => decimal.Parse(faker.Commerce.Price())));

    public Faker<Player> Player => _lazyPlayerFaker.Value;
    public Faker<Game> Game => _lazyGameFaker.Value;
}
