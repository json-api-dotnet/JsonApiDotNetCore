using System;
using Bogus;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    internal sealed class EagerLoadingFakers : FakerContainer
    {
        private readonly Lazy<Faker<State>> _lazyStateFaker = new Lazy<Faker<State>>(() =>
            new Faker<State>()
                .UseSeed(GetFakerSeed())
                .RuleFor(state => state.Name, f => f.Address.City()));

        private readonly Lazy<Faker<City>> _lazyCityFaker = new Lazy<Faker<City>>(() =>
            new Faker<City>()
                .UseSeed(GetFakerSeed())
                .RuleFor(city => city.Name, f => f.Address.City()));

        private readonly Lazy<Faker<Street>> _lazyStreetFaker = new Lazy<Faker<Street>>(() =>
            new Faker<Street>()
                .UseSeed(GetFakerSeed())
                .RuleFor(street => street.Name, f => f.Address.StreetName()));

        private readonly Lazy<Faker<Building>> _lazyBuildingFaker = new Lazy<Faker<Building>>(() =>
            new Faker<Building>()
                .UseSeed(GetFakerSeed())
                .RuleFor(building => building.Number, f => f.Address.BuildingNumber()));

        private readonly Lazy<Faker<Window>> _lazyWindowFaker = new Lazy<Faker<Window>>(() =>
            new Faker<Window>()
                .UseSeed(GetFakerSeed())
                .RuleFor(window => window.HeightInCentimeters, f => f.Random.Number(30, 199))
                .RuleFor(window => window.WidthInCentimeters, f => f.Random.Number(30, 199)));

        private readonly Lazy<Faker<Door>> _lazyDoorFaker = new Lazy<Faker<Door>>(() =>
            new Faker<Door>()
                .UseSeed(GetFakerSeed())
                .RuleFor(door => door.Color, f => f.Commerce.Color()));

        public Faker<State> State => _lazyStateFaker.Value;
        public Faker<City> City => _lazyCityFaker.Value;
        public Faker<Street> Street => _lazyStreetFaker.Value;
        public Faker<Building> Building => _lazyBuildingFaker.Value;
        public Faker<Window> Window => _lazyWindowFaker.Value;
        public Faker<Door> Door => _lazyDoorFaker.Value;
    }
}
