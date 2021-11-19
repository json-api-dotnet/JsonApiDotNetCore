using System;
using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class RestrictionFakers : FakerContainer
    {
        private readonly Lazy<Faker<Room>> _lazyRoomFaker = new(() =>
            new Faker<Room>()
                .UseSeed(GetFakerSeed())
                .RuleFor(room => room.WindowCount, faker => faker.Random.Int(0, 3)));

        private readonly Lazy<Faker<Table>> _lazyTableFaker = new(() =>
            new Faker<Table>()
                .UseSeed(GetFakerSeed())
                .RuleFor(table => table.LegCount, faker => faker.Random.Int(1, 4)));

        private readonly Lazy<Faker<Chair>> _lazyChairFaker = new(() =>
            new Faker<Chair>()
                .UseSeed(GetFakerSeed())
                .RuleFor(chair => chair.LegCount, faker => faker.Random.Int(2, 4)));

        private readonly Lazy<Faker<Sofa>> _lazySofaFaker = new(() =>
            new Faker<Sofa>()
                .UseSeed(GetFakerSeed())
                .RuleFor(sofa => sofa.SeatCount, faker => faker.Random.Int(2, 6)));

        private readonly Lazy<Faker<Pillow>> _lazyPillowFaker = new(() =>
            new Faker<Pillow>()
                .UseSeed(GetFakerSeed())
                .RuleFor(pillow => pillow.Color, faker => faker.Internet.Color()));

        private readonly Lazy<Faker<Bed>> _lazyBedFaker = new(() =>
            new Faker<Bed>()
                .UseSeed(GetFakerSeed())
                .RuleFor(bed => bed.IsDouble, faker => faker.Random.Bool()));

        public Faker<Room> Room => _lazyRoomFaker.Value;
        public Faker<Table> Table => _lazyTableFaker.Value;
        public Faker<Chair> Chair => _lazyChairFaker.Value;
        public Faker<Sofa> Sofa => _lazySofaFaker.Value;
        public Faker<Bed> Bed => _lazyBedFaker.Value;
        public Faker<Pillow> Pillow => _lazyPillowFaker.Value;
    }
}
