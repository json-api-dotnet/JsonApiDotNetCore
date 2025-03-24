using Bogus;
using JetBrains.Annotations;
using OpenApiTests.ResourceInheritance.Models;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ResourceInheritanceFakers
{
    private readonly Lazy<Faker<District>> _lazyDistrictFaker = new(() => new Faker<District>()
        .MakeDeterministic()
        .RuleFor(district => district.Name, faker => faker.Address.County()));

    private readonly Lazy<Faker<StaffMember>> _lazyStaffMemberFaker = new(() => new Faker<StaffMember>()
        .MakeDeterministic()
        .RuleFor(staffMember => staffMember.Name, faker => faker.Person.FullName));

    private readonly Lazy<Faker<Residence>> _lazyResidenceFaker = new(() => new Faker<Residence>()
        .MakeDeterministic()
        .RuleFor(residence => residence.SurfaceInSquareMeters, faker => faker.Random.Int(50, 250))
        .RuleFor(residence => residence.NumberOfResidents, faker => faker.Random.Int(0, 15)));

    private readonly Lazy<Faker<Mansion>> _lazyMansionFaker = new(() => new Faker<Mansion>()
        .MakeDeterministic()
        .RuleFor(mansion => mansion.SurfaceInSquareMeters, faker => faker.Random.Int(500, 2500))
        .RuleFor(mansion => mansion.NumberOfResidents, faker => faker.Random.Int(0, 150))
        .RuleFor(mansion => mansion.OwnerName, faker => faker.Person.FullName));

    private readonly Lazy<Faker<FamilyHome>> _lazyFamilyHomeFaker = new(() => new Faker<FamilyHome>()
        .MakeDeterministic()
        .RuleFor(familyHome => familyHome.SurfaceInSquareMeters, faker => faker.Random.Int(500, 2500))
        .RuleFor(familyHome => familyHome.NumberOfResidents, faker => faker.Random.Int(0, 150))
        .RuleFor(familyHome => familyHome.FloorCount, faker => faker.Random.Int(1, 4)));

    private readonly Lazy<Faker<Kitchen>> _lazyKitchenFaker = new(() => new Faker<Kitchen>()
        .MakeDeterministic()
        .RuleFor(kitchen => kitchen.SurfaceInSquareMeters, faker => faker.Random.Int(50, 250))
        .RuleFor(kitchen => kitchen.HasPantry, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Bedroom>> _lazyBedroomFaker = new(() => new Faker<Bedroom>()
        .MakeDeterministic()
        .RuleFor(bedroom => bedroom.SurfaceInSquareMeters, faker => faker.Random.Int(50, 250))
        .RuleFor(bedroom => bedroom.BedCount, faker => faker.Random.Int(1, 5)));

    private readonly Lazy<Faker<Bathroom>> _lazyBathroomFaker = new(() => new Faker<Bathroom>()
        .MakeDeterministic()
        .RuleFor(bathroom => bathroom.SurfaceInSquareMeters, faker => faker.Random.Int(50, 250))
        .RuleFor(bathroom => bathroom.HasBath, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<LivingRoom>> _lazyLivingRoomFaker = new(() => new Faker<LivingRoom>()
        .MakeDeterministic()
        .RuleFor(livingRoom => livingRoom.SurfaceInSquareMeters, faker => faker.Random.Int(50, 250))
        .RuleFor(livingRoom => livingRoom.HasDiningTable, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Toilet>> _lazyToiletFaker = new(() => new Faker<Toilet>()
        .MakeDeterministic()
        .RuleFor(toilet => toilet.SurfaceInSquareMeters, faker => faker.Random.Int(50, 250))
        .RuleFor(toilet => toilet.HasSink, faker => faker.Random.Bool()));

    private readonly Lazy<Faker<Road>> _lazyRoadFaker = new(() => new Faker<Road>()
        .MakeDeterministic()
        .RuleFor(road => road.LengthInMeters, faker => faker.Random.Int(20, 2500)));

    private readonly Lazy<Faker<CyclePath>> _lazyCyclePathFaker = new(() => new Faker<CyclePath>()
        .MakeDeterministic()
        .RuleFor(cyclePath => cyclePath.LengthInMeters, faker => faker.Random.Int(20, 2500))
        .RuleFor(cyclePath => cyclePath.HasLaneForPedestrians, faker => faker.Random.Bool()));

    public Faker<District> District => _lazyDistrictFaker.Value;
    public Faker<StaffMember> StaffMember => _lazyStaffMemberFaker.Value;
    public Faker<Residence> Residence => _lazyResidenceFaker.Value;
    public Faker<FamilyHome> FamilyHome => _lazyFamilyHomeFaker.Value;
    public Faker<Mansion> Mansion => _lazyMansionFaker.Value;
    public Faker<Kitchen> Kitchen => _lazyKitchenFaker.Value;
    public Faker<Bedroom> Bedroom => _lazyBedroomFaker.Value;
    public Faker<Bathroom> Bathroom => _lazyBathroomFaker.Value;
    public Faker<LivingRoom> LivingRoom => _lazyLivingRoomFaker.Value;
    public Faker<Toilet> Toilet => _lazyToiletFaker.Value;
    public Faker<Road> Road => _lazyRoadFaker.Value;
    public Faker<CyclePath> CyclePath => _lazyCyclePathFaker.Value;
}
