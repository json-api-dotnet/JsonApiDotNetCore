using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound;

public abstract class CompoundInheritanceWriteTests<TDbContext> : IClassFixture<IntegrationTestContext<TestableStartup<TDbContext>, TDbContext>>
    where TDbContext : CompoundInheritanceDbContext
{
    private readonly IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> _testContext;
    private readonly CompoundInheritanceFakers _fakers = new();

    protected CompoundInheritanceWriteTests(IntegrationTestContext<TestableStartup<TDbContext>, TDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ListingsController>();
        testContext.UseController<ResidencesController>();
        testContext.UseController<ApartmentsController>();
        testContext.UseController<HousesController>();
        testContext.UseController<ParkingsController>();
        testContext.UseController<ListingAgentsController>();
    }

    [Fact]
    public async Task Can_create_derived_with_inherited_and_own_compounds()
    {
        // Arrange
        Apartment newApartment = _fakers.Apartment.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                attributes = new
                {
                    title = newApartment.Title,
                    price = newApartment.Price,
                    address = new
                    {
                        street = newApartment.Address!.Street,
                        city = newApartment.Address.City,
                        countryCode = newApartment.Address.CountryCode
                    },
                    photos = newApartment.Photos,
                    features = newApartment.Features.Select(feature => new
                    {
                        name = feature.Name,
                        isPremium = feature.IsPremium
                    }),
                    rooms = newApartment.Rooms.Select(room => new
                    {
                        name = room.Name,
                        areaInSqm = room.AreaInSqm
                    }),
                    building = new
                    {
                        name = newApartment.Building!.Name,
                        floor = newApartment.Building.Floor,
                        hasElevator = newApartment.Building.HasElevator
                    },
                    insurance = new
                    {
                        premium = newApartment.Insurance!.Premium
                    }
                }
            }
        };

        const string route = "/apartments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("apartments");
            resource.GetAttribute("address").GetProperty("city").GetString().Should().Be(newApartment.Address.City);
            resource.GetAttribute("building").GetProperty("name").GetString().Should().Be(newApartment.Building.Name);
            resource.GetAttribute("building").GetProperty("floor").GetInt32().Should().Be(newApartment.Building.Floor);
            resource.GetAttribute("insurance").GetProperty("premium").GetDecimal().Should().Be(newApartment.Insurance.Premium);
            resource.GetAttribute("features").GetArrayLength().Should().Be(2);
            resource.GetAttribute("rooms").GetArrayLength().Should().Be(2);
            resource.GetAttribute("photos").GetArrayLength().Should().Be(2);
        });

        long newApartmentId = long.Parse(responseDocument.Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Apartment apartmentInDatabase = await dbContext.Apartments.FirstWithIdAsync(newApartmentId);

            apartmentInDatabase.Title.Should().Be(newApartment.Title);
            apartmentInDatabase.Address.Should().NotBeNull();
            apartmentInDatabase.Address.City.Should().Be(newApartment.Address.City);
            apartmentInDatabase.Photos.Should().Equal(newApartment.Photos);
            apartmentInDatabase.Features.Select(feature => feature.Name).Should().Equal(newApartment.Features.Select(feature => feature.Name));
            apartmentInDatabase.Rooms.Select(room => room.AreaInSqm).Should().Equal(newApartment.Rooms.Select(room => room.AreaInSqm));
            apartmentInDatabase.Building.Should().NotBeNull();
            apartmentInDatabase.Building.Floor.Should().Be(newApartment.Building.Floor);
            apartmentInDatabase.Insurance.Should().NotBeNull();
            apartmentInDatabase.Insurance.Premium.Should().Be(newApartment.Insurance.Premium);
        });
    }

    [Fact]
    public async Task Can_update_inherited_compound()
    {
        // Arrange
        Apartment existingApartment = _fakers.Apartment.GenerateOne();
        ListingAddress newAddress = _fakers.Apartment.GenerateOne().Address!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Apartments.Add(existingApartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                id = existingApartment.StringId,
                attributes = new
                {
                    address = new
                    {
                        street = newAddress.Street,
                        city = newAddress.City,
                        countryCode = newAddress.CountryCode
                    }
                }
            }
        };

        string route = $"/apartments/{existingApartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Apartment apartmentInDatabase = await dbContext.Apartments.FirstWithIdAsync(existingApartment.Id);

            apartmentInDatabase.Address.Should().NotBeNull();
            apartmentInDatabase.Address.Street.Should().Be(newAddress.Street);
            apartmentInDatabase.Address.City.Should().Be(newAddress.City);
            apartmentInDatabase.Address.CountryCode.Should().Be(newAddress.CountryCode);

            apartmentInDatabase.Building.Should().NotBeNull();
            apartmentInDatabase.Building.Name.Should().Be(existingApartment.Building!.Name);
        });
    }

    [Fact]
    public async Task Can_update_derived_compound()
    {
        // Arrange
        Apartment existingApartment = _fakers.Apartment.GenerateOne();
        BuildingInfo newBuilding = _fakers.Apartment.GenerateOne().Building!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Apartments.Add(existingApartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                id = existingApartment.StringId,
                attributes = new
                {
                    building = new
                    {
                        name = newBuilding.Name,
                        floor = newBuilding.Floor,
                        hasElevator = newBuilding.HasElevator
                    }
                }
            }
        };

        string route = $"/apartments/{existingApartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Apartment apartmentInDatabase = await dbContext.Apartments.FirstWithIdAsync(existingApartment.Id);

            apartmentInDatabase.Building.Should().NotBeNull();
            apartmentInDatabase.Building.Name.Should().Be(newBuilding.Name);
            apartmentInDatabase.Building.Floor.Should().Be(newBuilding.Floor);
            apartmentInDatabase.Building.HasElevator.Should().Be(newBuilding.HasElevator);

            apartmentInDatabase.Address.Should().NotBeNull();
            apartmentInDatabase.Address.City.Should().Be(existingApartment.Address!.City);
        });
    }

    [Fact]
    public async Task Can_update_derived_resource_at_base_endpoint()
    {
        // Arrange
        Apartment existingApartment = _fakers.Apartment.GenerateOne();
        BuildingInfo newBuilding = _fakers.Apartment.GenerateOne().Building!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Listings.Add(existingApartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                id = existingApartment.StringId,
                attributes = new
                {
                    building = new
                    {
                        name = newBuilding.Name,
                        floor = newBuilding.Floor,
                        hasElevator = newBuilding.HasElevator
                    }
                }
            }
        };

        string route = $"/listings/{existingApartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Apartment apartmentInDatabase = await dbContext.Apartments.FirstWithIdAsync(existingApartment.Id);

            apartmentInDatabase.Building.Should().NotBeNull();
            apartmentInDatabase.Building.Name.Should().Be(newBuilding.Name);
            apartmentInDatabase.Building.Floor.Should().Be(newBuilding.Floor);
        });
    }

    [Fact]
    public async Task Cannot_set_sibling_type_compound()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                attributes = new
                {
                    title = "Some apartment",
                    garden = new
                    {
                        areaInSqm = 100
                    }
                }
            }
        };

        const string route = "/apartments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be("Attribute 'garden' does not exist on resource type 'apartments'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/garden");
    }

    [Fact]
    public async Task Reports_nested_pointer_for_inherited_compound()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                attributes = new
                {
                    title = "Some apartment",
                    address = new
                    {
                        street = "Main street"
                    }
                }
            }
        };

        const string route = "/apartments";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The City field is required.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/address/city");
    }

    [Fact]
    public async Task Can_patch_identical_inherited_compound_and_get_204()
    {
        // Arrange
        Apartment existingApartment = _fakers.Apartment.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Apartments.Add(existingApartment);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "apartments",
                id = existingApartment.StringId,
                attributes = new
                {
                    address = new
                    {
                        street = existingApartment.Address!.Street,
                        city = existingApartment.Address.City,
                        countryCode = existingApartment.Address.CountryCode
                    }
                }
            }
        };

        string route = $"/apartments/{existingApartment.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Apartment apartmentInDatabase = await dbContext.Apartments.FirstWithIdAsync(existingApartment.Id);

            apartmentInDatabase.Address.Should().NotBeNull();
            apartmentInDatabase.Address.City.Should().Be(existingApartment.Address.City);
            apartmentInDatabase.Title.Should().Be(existingApartment.Title);
        });
    }
}
