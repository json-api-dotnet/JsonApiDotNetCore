using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound;

public abstract class CompoundInheritanceReadTests<TDbContext> : IClassFixture<IntegrationTestContext<TestableStartup<TDbContext>, TDbContext>>
    where TDbContext : CompoundInheritanceDbContext
{
    private readonly IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> _testContext;
    private readonly CompoundInheritanceFakers _fakers = new();

    protected CompoundInheritanceReadTests(IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ListingsController>();
        testContext.UseController<ResidencesController>();
        testContext.UseController<ApartmentsController>();
        testContext.UseController<HousesController>();
        testContext.UseController<ParkingsController>();
        testContext.UseController<ListingAgentsController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;
        options.AllowUnknownQueryStringParameters = true;
    }

    [Fact]
    public async Task Can_get_abstract_collection_with_derived_compounds()
    {
        // Arrange
        (Apartment apartment, House house, Parking parking) = await SeedListingsAsync();

        const string route = "/listings";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "apartments").Subject.With(resource =>
        {
            resource.Id.Should().Be(apartment.StringId);

            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "building", "insurance");
            resource.GetAttribute("address").GetProperty("city").GetString().Should().Be(apartment.Address!.City);
            resource.GetAttribute("building").GetProperty("name").GetString().Should().Be(apartment.Building!.Name);
            resource.GetAttribute("building").GetProperty("floor").GetInt32().Should().Be(apartment.Building.Floor);
            resource.GetAttribute("insurance").GetProperty("premium").GetDecimal().Should().Be(apartment.Insurance!.Premium);
            resource.GetAttribute("features").GetArrayLength().Should().Be(2);
            resource.GetAttribute("rooms").GetArrayLength().Should().Be(2);
            resource.GetAttribute("photos").GetArrayLength().Should().Be(2);
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "houses").Subject.With(resource =>
        {
            resource.Id.Should().Be(house.StringId);

            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "garden", "insurance");
            resource.GetAttribute("garden").GetProperty("areaInSqm").GetInt32().Should().Be(house.Garden!.AreaInSqm);
            resource.GetAttribute("garden").GetProperty("trees").GetArrayLength().Should().Be(2);
            resource.GetAttribute("insurance").GetProperty("premium").GetDecimal().Should().Be(house.Insurance!.Premium);
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "parkings").Subject.With(resource =>
        {
            resource.Id.Should().Be(parking.StringId);

            resource.Attributes.Should().OnlyContainKeys("title", "price", "dimensions", "insurance");
            resource.GetAttribute("dimensions").GetProperty("width").GetDecimal().Should().Be(parking.Dimensions!.Width);
            resource.GetAttribute("dimensions").GetProperty("depth").GetDecimal().Should().Be(parking.Dimensions.Depth);
            resource.GetAttribute("insurance").GetProperty("premium").GetDecimal().Should().Be(parking.Insurance!.Premium);
        });
    }

    [Fact]
    public async Task Can_get_abstract_resource_by_id()
    {
        // Arrange
        (Apartment apartment, _, _) = await SeedListingsAsync();

        string route = $"/listings/{apartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("apartments");
            resource.Id.Should().Be(apartment.StringId);

            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "building", "insurance");
            resource.GetAttribute("address").GetProperty("city").GetString().Should().Be(apartment.Address!.City);
            resource.GetAttribute("building").GetProperty("name").GetString().Should().Be(apartment.Building!.Name);
        });
    }

    [Fact]
    public async Task Can_get_derived_resource()
    {
        // Arrange
        (Apartment apartment, _, _) = await SeedListingsAsync();

        string route = $"/apartments/{apartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("apartments");
            resource.Id.Should().Be(apartment.StringId);

            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "building", "insurance");
            resource.GetAttribute("address").GetProperty("street").GetString().Should().Be(apartment.Address!.Street);
            resource.GetAttribute("photos").EnumerateArray().Select(element => element.GetString()).Should().Equal(apartment.Photos);
            resource.GetAttribute("features").EnumerateArray().Select(element => element.GetProperty("name").GetString()).Should()
                .Equal(apartment.Features.Select(feature => feature.Name));

            resource.GetAttribute("rooms").EnumerateArray().Select(element => element.GetProperty("areaInSqm").GetInt32()).Should()
                .Equal(apartment.Rooms.Select(room => room.AreaInSqm));

            resource.GetAttribute("building").GetProperty("hasElevator").GetBoolean().Should().Be(apartment.Building!.HasElevator);
        });
    }

    [Fact]
    public async Task Can_select_inherited_compound_leaf_at_derived_endpoint()
    {
        // Arrange
        (Apartment apartment, _, _) = await SeedListingsAsync();

        const string route = "/apartments?fields[apartments]=address.city";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        ResourceObject resource = responseDocument.Data.ManyValue[0];
        resource.Id.Should().Be(apartment.StringId);
        resource.Attributes.Should().OnlyContainKeys("address");

        JsonElement address = resource.GetAttribute("address");
        address.EnumerateObject().Select(property => property.Name).Should().Equal("city");
        address.GetProperty("city").GetString().Should().Be(apartment.Address!.City);
    }

    [Fact]
    public async Task Can_select_derived_fieldset_at_abstract_endpoint()
    {
        // Arrange
        (Apartment apartment, House house, Parking parking) = await SeedListingsAsync();

        const string route = "/listings?fields[apartments]=building.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "apartments").Subject.With(resource =>
        {
            resource.Id.Should().Be(apartment.StringId);
            resource.Attributes.Should().OnlyContainKeys("building");

            JsonElement building = resource.GetAttribute("building");
            building.EnumerateObject().Select(property => property.Name).Should().Equal("name");
            building.GetProperty("name").GetString().Should().Be(apartment.Building!.Name);
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "houses").Subject.With(resource =>
        {
            resource.Id.Should().Be(house.StringId);
            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "garden", "insurance");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "parkings").Subject.With(resource =>
        {
            resource.Id.Should().Be(parking.StringId);
            resource.Attributes.Should().OnlyContainKeys("title", "price", "dimensions", "insurance");
        });
    }

    [Fact]
    public async Task Does_not_apply_fieldset_of_abstract_intermediate_type_to_derived_types()
    {
        // Arrange
        await SeedListingsAsync();

        const string route = "/listings?fields[residences]=address.city";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "apartments").Subject.With(resource =>
        {
            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "building", "insurance");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "houses").Subject.With(resource =>
        {
            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "garden", "insurance");
        });
    }

    [Fact]
    public async Task Can_select_inherited_leaf_for_all_derived_types()
    {
        // Arrange
        (Apartment apartment, House house, Parking parking) = await SeedListingsAsync();

        const string route = "/listings?fields[apartments]=address.city&fields[houses]=address.city";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "apartments").Subject.With(resource =>
        {
            resource.Id.Should().Be(apartment.StringId);
            resource.Attributes.Should().OnlyContainKeys("address");
            resource.GetAttribute("address").EnumerateObject().Select(property => property.Name).Should().Equal("city");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "houses").Subject.With(resource =>
        {
            resource.Id.Should().Be(house.StringId);
            resource.Attributes.Should().OnlyContainKeys("address");
            resource.GetAttribute("address").EnumerateObject().Select(property => property.Name).Should().Equal("city");
        });

        responseDocument.Data.ManyValue.Should().ContainSingle(resource => resource.Type == "parkings").Subject.With(resource =>
        {
            resource.Id.Should().Be(parking.StringId);
            resource.Attributes.Should().OnlyContainKeys("title", "price", "dimensions", "insurance");
        });
    }

    [Fact]
    public async Task Can_filter_on_inherited_compound_at_derived_endpoint()
    {
        // Arrange
        (Apartment apartment, _, _) = await SeedListingsAsync();

        Apartment otherApartment = _fakers.Apartment.GenerateOne();
        otherApartment.Address!.City = "Rotterdam";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Apartments.Add(otherApartment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/apartments?filter=equals(address.city,'{apartment.Address!.City}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(apartment.StringId);
    }

    [Fact]
    public async Task Can_filter_derived_only_compound_with_isType()
    {
        // Arrange
        (Apartment apartment, _, _) = await SeedListingsAsync();

        Apartment otherApartment = _fakers.Apartment.GenerateOne();
        otherApartment.Building!.Name = "OtherTower";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Apartments.Add(otherApartment);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/listings?filter=isType(,apartments,equals(building.name,'{apartment.Building!.Name}'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("apartments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(apartment.StringId);
    }

    [Fact]
    public async Task Cannot_filter_derived_only_compound_without_isType()
    {
        // Arrange
        const string route = "/listings?filter=equals(building.name,'Tower')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified filter is invalid.");
        error.Detail.Should().Be("Field 'building' does not exist on resource type 'listings'. Failed at position 8: equals(^building.name,'Tower')");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("filter");
    }

    [Fact]
    public async Task Can_sort_on_derived_only_compound_at_abstract_endpoint()
    {
        // Arrange
        (Apartment apartment, House house, Parking parking) = await SeedListingsAsync();

        Apartment otherApartment = _fakers.Apartment.GenerateOne();
        apartment.Building!.Name = "Beta";
        otherApartment.Building!.Name = "Alpha";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Apartments.Update(apartment);
            dbContext.Apartments.Add(otherApartment);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/listings?sort=building.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        // Ascending order places NULL values (non-apartments) last in PostgreSQL.
        responseDocument.Data.ManyValue[0].Id.Should().Be(otherApartment.StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(apartment.StringId);

        responseDocument.Data.ManyValue.Skip(2).Select(resource => resource.Id).Should().BeEquivalentTo(new[]
        {
            house.StringId,
            parking.StringId
        });
    }

    [Fact]
    public async Task Cannot_sort_on_ambiguous_derived_member()
    {
        // Arrange
        const string route = "/listings?sort=insurance.premium";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be("Field 'insurance' is defined on multiple types that derive from resource type 'listings'. " +
            "Failed at position 1: ^insurance.premium");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Fact]
    public async Task Can_filter_on_inherited_compound_collection()
    {
        // Arrange
        (Apartment apartment, House house, _) = await SeedListingsAsync();

        apartment.Features[0].Name = "sauna";
        house.Features[0].Name = "pool";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Residences.UpdateRange(apartment, house);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/residences?filter=has(features,equals(name,'sauna'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("apartments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(apartment.StringId);
    }

    [Fact]
    public async Task Can_filter_on_inherited_scalar_collection_count()
    {
        // Arrange
        (Apartment apartment, House house, _) = await SeedListingsAsync();

        house.Photos.Add("extra.jpg");

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Houses.Update(house);
            await dbContext.SaveChangesAsync();
        });

        apartment.Photos.Should().HaveCount(2);

        const string route = "/residences?filter=equals(count(photos),'3')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("houses");
        responseDocument.Data.ManyValue[0].Id.Should().Be(house.StringId);
    }

    [Fact]
    public async Task Can_filter_on_inherited_scalar_collection_elements()
    {
        // Arrange
        (Apartment apartment, House house, _) = await SeedListingsAsync();

        apartment.Photos[0] = "special-view.jpg";
        house.Photos[0] = "front.jpg";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Residences.UpdateRange(apartment, house);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/residences?filter=has(photos,startsWith(_,'special'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("apartments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(apartment.StringId);
    }

    [Theory]
    [InlineData("/apartments")]
    [InlineData("/residences")]
    public async Task Can_filter_collection_declared_on_intermediate_abstract_type(string endpoint)
    {
        // Arrange
        (Apartment apartment, House house, _) = await SeedListingsAsync();

        apartment.Rooms[0].Name = "kitchen";
        house.Rooms[0].Name = "garage";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Residences.UpdateRange(apartment, house);
            await dbContext.SaveChangesAsync();
        });

        string route = $"{endpoint}?filter=has(rooms,equals(name,'kitchen'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("apartments");
        responseDocument.Data.ManyValue[0].Id.Should().Be(apartment.StringId);
    }

    [Fact]
    public async Task Can_include_polymorphic_children_with_compounds()
    {
        // Arrange
        (Apartment apartment, House house, Parking parking) = await SeedListingsAsync(withAgent: true);

        const string route = "/listingAgents?include=listings&fields[apartments]=building.name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("listingAgents");

        responseDocument.Included.Should().HaveCount(3);

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "apartments").Subject.With(resource =>
        {
            resource.Id.Should().Be(apartment.StringId);
            resource.Attributes.Should().OnlyContainKeys("building");
            resource.GetAttribute("building").EnumerateObject().Select(property => property.Name).Should().Equal("name");
        });

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "houses").Subject.With(resource =>
        {
            resource.Id.Should().Be(house.StringId);
            resource.Attributes.Should().OnlyContainKeys("title", "price", "address", "photos", "features", "rooms", "garden", "insurance");
            resource.GetAttribute("garden").GetProperty("areaInSqm").GetInt32().Should().Be(house.Garden!.AreaInSqm);
        });

        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "parkings").Subject.With(resource =>
        {
            resource.Id.Should().Be(parking.StringId);
            resource.Attributes.Should().OnlyContainKeys("title", "price", "dimensions", "insurance");
            resource.GetAttribute("dimensions").GetProperty("width").GetDecimal().Should().Be(parking.Dimensions!.Width);
        });
    }

    [Fact]
    public async Task Can_filter_included_polymorphic_collection()
    {
        // Arrange
        (Apartment apartment, House house, Parking parking) = await SeedListingsAsync(withAgent: true);

        apartment.Price = 500_000;
        house.Price = 800_000;
        parking.Price = 20_000;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Listings.UpdateRange(apartment, house, parking);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/listingAgents?include=listings&filter[listings]=greaterThan(price,'600000')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("houses");
        responseDocument.Included[0].Id.Should().Be(house.StringId);
    }

    private async Task<(Apartment Apartment, House House, Parking Parking)> SeedListingsAsync(bool withAgent = false)
    {
        Apartment apartment = _fakers.Apartment.GenerateOne();
        apartment.Building!.Name = "Tower";

        House house = _fakers.House.GenerateOne();
        Parking parking = _fakers.Parking.GenerateOne();

        ListingAgent agent = _fakers.ListingAgent.GenerateOne();

        if (withAgent)
        {
            agent.Listings = new HashSet<Listing>([apartment, house, parking]);
        }

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Listing>();
            await dbContext.ClearTableAsync<ListingAgent>();

            dbContext.Listings.AddRange(apartment, house, parking);
            dbContext.ListingAgents.Add(agent);

            await dbContext.SaveChangesAsync();
        });

        return (apartment, house, parking);
    }
}
