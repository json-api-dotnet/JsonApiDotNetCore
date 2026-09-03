using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CoffeeFakers
{
    private readonly Lazy<Faker<CupOfCoffee>> _lazyCupOfCoffeeFaker = new(() => new Faker<CupOfCoffee>()
        .MakeDeterministic()
        .RuleFor(cupOfCoffee => cupOfCoffee.HasSugar, faker => faker.Random.Bool())
        .RuleFor(cupOfCoffee => cupOfCoffee.HasMilk, faker => faker.Random.Bool()));

    public Faker<CupOfCoffee> CupOfCoffee => _lazyCupOfCoffeeFaker.Value;
}
