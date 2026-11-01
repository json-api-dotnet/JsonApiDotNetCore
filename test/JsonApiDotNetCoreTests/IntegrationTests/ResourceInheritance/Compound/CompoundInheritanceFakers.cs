using Bogus;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound;

internal sealed class CompoundInheritanceFakers
{
    private readonly Lazy<Faker<ListingAddress>> _lazyAddressFaker = new(() => new Faker<ListingAddress>()
        .MakeDeterministic()
        .RuleFor(address => address.Street, faker => faker.Address.StreetName())
        .RuleFor(address => address.City, faker => faker.Address.City())
        .RuleFor(address => address.CountryCode, faker => faker.Address.CountryCode()));

    private readonly Lazy<Faker<ListingFeature>> _lazyFeatureFaker = new(() => new Faker<ListingFeature>()
        .MakeDeterministic()
        .RuleFor(feature => feature.Name, faker => faker.Lorem.Word())
        .RuleFor(feature => feature.IsPremium, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Room>> _lazyRoomFaker = new(() => new Faker<Room>()
        .MakeDeterministic()
        .RuleFor(room => room.Name, faker => faker.Lorem.Word())
        .RuleFor(room => room.AreaInSqm, faker => faker.Random.Int(5, 60)));

    private readonly Lazy<Faker<BuildingInfo>> _lazyBuildingFaker = new(() => new Faker<BuildingInfo>()
        .MakeDeterministic()
        .RuleFor(building => building.Name, faker => faker.Lorem.Word())
        .RuleFor(building => building.Floor, faker => faker.Random.Int(1, 40))
        .RuleFor(building => building.HasElevator, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<HomeInsurance>> _lazyHomeInsuranceFaker = new(() => new Faker<HomeInsurance>()
        .MakeDeterministic()
        .RuleFor(insurance => insurance.Premium, faker => faker.Random.Int(100, 900)));

    private readonly Lazy<Faker<GardenInfo>> _lazyGardenFaker = new(() => new Faker<GardenInfo>()
        .MakeDeterministic()
        .RuleFor(garden => garden.AreaInSqm, faker => faker.Random.Int(10, 500))
        .RuleFor(garden => garden.Trees, faker => [.. faker.Make(2, () => faker.Lorem.Word())]));

    private readonly Lazy<Faker<ParkingDimensions>> _lazyDimensionsFaker = new(() => new Faker<ParkingDimensions>()
        .MakeDeterministic()
        .RuleFor(dimensions => dimensions.Width, faker => faker.Random.Int(2, 4))
        .RuleFor(dimensions => dimensions.Depth, faker => faker.Random.Int(4, 8)));

    private readonly Lazy<Faker<ParkingInsurance>> _lazyParkingInsuranceFaker = new(() => new Faker<ParkingInsurance>()
        .MakeDeterministic()
        .RuleFor(insurance => insurance.Premium, faker => faker.Random.Int(10, 90)));

    private readonly Lazy<Faker<Apartment>> _lazyApartmentFaker;
    private readonly Lazy<Faker<House>> _lazyHouseFaker;
    private readonly Lazy<Faker<Parking>> _lazyParkingFaker;

    private readonly Lazy<Faker<ListingAgent>> _lazyListingAgentFaker = new(() => new Faker<ListingAgent>()
        .MakeDeterministic()
        .RuleFor(agent => agent.Name, faker => faker.Person.FullName));

    public Faker<Apartment> Apartment => _lazyApartmentFaker.Value;
    public Faker<House> House => _lazyHouseFaker.Value;
    public Faker<Parking> Parking => _lazyParkingFaker.Value;
    public Faker<ListingAgent> ListingAgent => _lazyListingAgentFaker.Value;

    public CompoundInheritanceFakers()
    {
        _lazyApartmentFaker = new Lazy<Faker<Apartment>>(() => new Faker<Apartment>()
            .MakeDeterministic()
            .RuleFor(apartment => apartment.Title, faker => faker.Lorem.Sentence(3))
            .RuleFor(apartment => apartment.Price, faker => faker.Random.Int(100_000, 900_000))
            .RuleFor(apartment => apartment.Address, _ => _lazyAddressFaker.Value.GenerateOne())
            .RuleFor(apartment => apartment.Photos, faker => [.. faker.Make(2, () => faker.Internet.UrlWithPath())])
            .RuleFor(apartment => apartment.Features, _ => _lazyFeatureFaker.Value.GenerateList(2))
            .RuleFor(apartment => apartment.Rooms, _ => _lazyRoomFaker.Value.GenerateList(2))
            .RuleFor(apartment => apartment.Building, _ => _lazyBuildingFaker.Value.GenerateOne())
            .RuleFor(apartment => apartment.Insurance, _ => _lazyHomeInsuranceFaker.Value.GenerateOne()));

        _lazyHouseFaker = new Lazy<Faker<House>>(() => new Faker<House>()
            .MakeDeterministic()
            .RuleFor(house => house.Title, faker => faker.Lorem.Sentence(3))
            .RuleFor(house => house.Price, faker => faker.Random.Int(200_000, 1_500_000))
            .RuleFor(house => house.Address, _ => _lazyAddressFaker.Value.GenerateOne())
            .RuleFor(house => house.Photos, faker => [.. faker.Make(2, () => faker.Internet.UrlWithPath())])
            .RuleFor(house => house.Features, _ => _lazyFeatureFaker.Value.GenerateList(2))
            .RuleFor(house => house.Rooms, _ => _lazyRoomFaker.Value.GenerateList(2))
            .RuleFor(house => house.Garden, _ => _lazyGardenFaker.Value.GenerateOne())
            .RuleFor(house => house.Insurance, _ => _lazyHomeInsuranceFaker.Value.GenerateOne()));

        _lazyParkingFaker = new Lazy<Faker<Parking>>(() => new Faker<Parking>()
            .MakeDeterministic()
            .RuleFor(parking => parking.Title, faker => faker.Lorem.Sentence(3))
            .RuleFor(parking => parking.Price, faker => faker.Random.Int(10_000, 50_000))
            .RuleFor(parking => parking.Dimensions, _ => _lazyDimensionsFaker.Value.GenerateOne())
            .RuleFor(parking => parking.Insurance, _ => _lazyParkingInsuranceFaker.Value.GenerateOne()));
    }
}
