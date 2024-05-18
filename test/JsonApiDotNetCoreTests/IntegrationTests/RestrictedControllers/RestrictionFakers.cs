using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class RestrictionFakers
{
    private readonly Lazy<Faker<Room>> _lazyRoomFaker = new(() => new Faker<Room>()
        .MakeDeterministic()
        .RuleFor(room => room.WindowCount, faker => faker.Random.Int(0, 3)));

    private readonly Lazy<Faker<Table>> _lazyTableFaker = new(() => new Faker<Table>()
        .MakeDeterministic()
        .RuleFor(table => table.LegCount, faker => faker.Random.Int(1, 4)));

    private readonly Lazy<Faker<Chair>> _lazyChairFaker = new(() => new Faker<Chair>()
        .MakeDeterministic()
        .RuleFor(chair => chair.LegCount, faker => faker.Random.Int(2, 4)));

    private readonly Lazy<Faker<Sofa>> _lazySofaFaker = new(() => new Faker<Sofa>()
        .MakeDeterministic()
        .RuleFor(sofa => sofa.SeatCount, faker => faker.Random.Int(2, 6)));

    private readonly Lazy<Faker<Pillow>> _lazyPillowFaker = new(() => new Faker<Pillow>()
        .MakeDeterministic()
        .RuleFor(pillow => pillow.Color, faker => faker.Internet.Color()));

    private readonly Lazy<Faker<Bed>> _lazyBedFaker = new(() => new Faker<Bed>()
        .MakeDeterministic()
        .RuleFor(bed => bed.IsDouble, faker => faker.Random.Bool()));

    public Faker<Room> Room => _lazyRoomFaker.Value;
    public Faker<Table> Table => _lazyTableFaker.Value;
    public Faker<Chair> Chair => _lazyChairFaker.Value;
    public Faker<Sofa> Sofa => _lazySofaFaker.Value;
    public Faker<Bed> Bed => _lazyBedFaker.Value;
    public Faker<Pillow> Pillow => _lazyPillowFaker.Value;
}
