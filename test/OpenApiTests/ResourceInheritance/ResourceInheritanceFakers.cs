using Bogus;
using JetBrains.Annotations;
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

    public Faker<District> District => _lazyDistrictFaker.Value;
    public Faker<Residence> Residence => _lazyResidenceFaker.Value;
    public Faker<FamilyHome> FamilyHome => _lazyFamilyHomeFaker.Value;
    public Faker<Mansion> Mansion => _lazyMansionFaker.Value;
}
