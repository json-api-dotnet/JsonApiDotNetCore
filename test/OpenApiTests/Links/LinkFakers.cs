using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LinkFakers : FakerContainer
{
    private readonly Lazy<Faker<Vacation>> _lazyVacationFaker = new(() => new Faker<Vacation>()
        .UseSeed(GetFakerSeed())
        .RuleFor(vacation => vacation.StartsAt, faker => faker.Date.Future())
        .RuleFor(vacation => vacation.EndsAt, faker => faker.Date.Future()));

    private readonly Lazy<Faker<Accommodation>> _lazyAccommodationFaker = new(() => new Faker<Accommodation>()
        .UseSeed(GetFakerSeed())
        .RuleFor(accommodation => accommodation.Address, faker => faker.Address.FullAddress()));

    private readonly Lazy<Faker<Transport>> _lazyTransportFaker = new(() => new Faker<Transport>()
        .UseSeed(GetFakerSeed())
        .RuleFor(transport => transport.Type, faker => faker.PickRandom<TransportType>())
        .RuleFor(transport => transport.DurationInMinutes, faker => faker.Random.Int(30, 24 * 60)));

    private readonly Lazy<Faker<Excursion>> _lazyExcursionFaker = new(() => new Faker<Excursion>()
        .UseSeed(GetFakerSeed())
        .RuleFor(excursion => excursion.OccursAt, faker => faker.Date.Future())
        .RuleFor(excursion => excursion.Description, faker => faker.Lorem.Sentence()));

    public Faker<Vacation> Vacation => _lazyVacationFaker.Value;
    public Faker<Accommodation> Accommodation => _lazyAccommodationFaker.Value;
    public Faker<Transport> Transport => _lazyTransportFaker.Value;
    public Faker<Excursion> Excursion => _lazyExcursionFaker.Value;
}
