using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.MixedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class MixedControllerFakers
{
    private readonly Lazy<Faker<CupOfCoffee>> _lazyCupOfCoffeeFaker = new(() => new Faker<CupOfCoffee>()
        .MakeDeterministic()
        .RuleFor(cupOfCoffee => cupOfCoffee.HasSugar, faker => faker.Random.Bool())
        .RuleFor(cupOfCoffee => cupOfCoffee.HasMilk, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Email>> _lazyEmailFaker = new(() => new Faker<Email>()
        .MakeDeterministic()
        .RuleFor(email => email.Subject, faker => faker.Lorem.Sentence())
        .RuleFor(email => email.Body, faker => faker.Lorem.Paragraphs())
        .RuleFor(email => email.From, faker => faker.Internet.Email())
        .RuleFor(email => email.To, faker => faker.Internet.Email()));

    public Faker<CupOfCoffee> CupOfCoffee => _lazyCupOfCoffeeFaker.Value;
    public Faker<Email> Email => _lazyEmailFaker.Value;
}
