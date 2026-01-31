using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenApiNSwagEndToEndTests.SubsetOfOperationsInheritance.GeneratedCode;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using OpenApiTests.ResourceInheritance.Models;
using OpenApiTests.ResourceInheritance.SubsetOfOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.ResourceInheritance.SubsetOfOperations;

public sealed class OperationsInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ResourceInheritanceFakers _fakers = new();

    public OperationsInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseInheritanceControllers(true);

        testContext.ConfigureServices(services =>
        {
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));

            services.AddSingleton<IJsonApiEndpointFilter, SubsetOfOperationsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, SubsetOfOperationsOperationFilter>();
        });
    }

    [Fact]
    public async Task Can_use_inheritance_at_operations_endpoint()
    {
        // Arrange
        StaffMember existingStaffMember = _fakers.StaffMember.GenerateOne();

        Mansion newMansion = _fakers.Mansion.GenerateOne();
        Kitchen newKitchen = _fakers.Kitchen.GenerateOne();
        FamilyHome newFamilyHome = _fakers.FamilyHome.GenerateOne();
        Bedroom newBedroom = _fakers.Bedroom.GenerateOne();
        int? newFamilyHomeSurfaceInSquareMeters = _fakers.FamilyHome.GenerateOne().SurfaceInSquareMeters;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.StaffMembers.Add(existingStaffMember);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new SubsetOfOperationsInheritanceClient(httpClient);

        const string mansionLid = "mansion-lid";
        const string kitchenLid = "kitchen-lid";
        const string familyHomeLid = "family-home-lid";
        const string bedroomLid = "bedroom-lid";

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                // NOTE: CreateBuildingOperation is not generated, because it is turned off.
                new CreateResidenceOperation
                {
                    Data = new DataInCreateMansionRequest
                    {
                        Lid = mansionLid,
                        Attributes = new AttributesInCreateMansionRequest
                        {
                            SurfaceInSquareMeters = newMansion.SurfaceInSquareMeters!.Value,
                            NumberOfResidents = newMansion.NumberOfResidents!.Value,
                            OwnerName = newMansion.OwnerName
                        },
                        Relationships = new RelationshipsInCreateMansionRequest
                        {
                            Staff = new ToManyStaffMemberInRequest
                            {
                                Data =
                                [
                                    new StaffMemberIdentifierInRequest
                                    {
                                        Id = existingStaffMember.StringId
                                    }
                                ]
                            }
                        }
                    }
                },
                // NOTE: It is possible to create an operation for abstract type.
                new CreateRoomOperation
                {
                    // NOTE: DataInCreateRoomRequest is generated, but as abstract type.
                    Data = new DataInCreateKitchenRequest
                    {
                        Lid = kitchenLid,
                        Attributes = new AttributesInCreateKitchenRequest
                        {
                            SurfaceInSquareMeters = newKitchen.SurfaceInSquareMeters!.Value,
                            HasPantry = newKitchen.HasPantry!.Value
                        },
                        Relationships = new RelationshipsInCreateKitchenRequest
                        {
                            Residence = new ToOneResidenceInRequest
                            {
                                Data = new MansionIdentifierInRequest
                                {
                                    Lid = mansionLid
                                }
                            }
                        }
                    }
                },
                new CreateFamilyHomeOperation
                {
                    Data = new DataInCreateFamilyHomeRequest
                    {
                        Lid = familyHomeLid,
                        Attributes = new AttributesInCreateFamilyHomeRequest
                        {
                            SurfaceInSquareMeters = newFamilyHome.SurfaceInSquareMeters!.Value,
                            NumberOfResidents = newFamilyHome.NumberOfResidents!.Value,
                            FloorCount = newFamilyHome.FloorCount!.Value
                        }
                    }
                },
                new CreateBedroomOperation
                {
                    Data = new DataInCreateBedroomRequest
                    {
                        Lid = bedroomLid,
                        Attributes = new AttributesInCreateBedroomRequest
                        {
                            SurfaceInSquareMeters = newBedroom.SurfaceInSquareMeters!.Value,
                            BedCount = newBedroom.BedCount!.Value
                        },
                        Relationships = new RelationshipsInCreateBedroomRequest
                        {
                            Residence = new ToOneResidenceInRequest
                            {
                                Data = new FamilyHomeIdentifierInRequest
                                {
                                    Lid = familyHomeLid
                                }
                            }
                        }
                    }
                },
                new AddToFamilyHomeRoomsRelationshipOperation
                {
                    Ref = new FamilyHomeRoomsRelationshipIdentifier
                    {
                        Lid = familyHomeLid
                    },
                    Data =
                    [
                        new KitchenIdentifierInRequest
                        {
                            Lid = kitchenLid
                        }
                    ]
                },
                new UpdateResidenceOperation
                {
                    // NOTE: Can use Ref to base type, while Data is derived.
                    Ref = new ResidenceIdentifierInRequest
                    {
                        Lid = familyHomeLid
                    },
                    Data = new DataInUpdateFamilyHomeRequest
                    {
                        Lid = familyHomeLid,
                        Attributes = new AttributesInUpdateFamilyHomeRequest
                        {
                            SurfaceInSquareMeters = newFamilyHomeSurfaceInSquareMeters
                        }
                    }
                },
                new RemoveFromMansionStaffRelationshipOperation
                {
                    Ref = new MansionStaffRelationshipIdentifier
                    {
                        Lid = mansionLid
                    },
                    Data = []
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        response.Should().NotBeNull();
        response.Atomic_results.Should().HaveCount(7);

        DataInMansionResponse mansionData = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<DataInMansionResponse>().Subject;
        AttributesInMansionResponse mansionAttributes = mansionData.Attributes.Should().BeOfType<AttributesInMansionResponse>().Subject;
        mansionAttributes.SurfaceInSquareMeters.Should().Be(newMansion.SurfaceInSquareMeters);
        mansionAttributes.NumberOfResidents.Should().Be(newMansion.NumberOfResidents);
        mansionAttributes.OwnerName.Should().Be(newMansion.OwnerName);
        mansionData.Relationships.Should().BeNull();

        DataInKitchenResponse kitchenData = response.Atomic_results.ElementAt(1).Data.Should().BeOfType<DataInKitchenResponse>().Subject;
        AttributesInKitchenResponse kitchenAttributes = kitchenData.Attributes.Should().BeOfType<AttributesInKitchenResponse>().Subject;
        kitchenAttributes.SurfaceInSquareMeters.Should().Be(newKitchen.SurfaceInSquareMeters);
        kitchenAttributes.HasPantry.Should().Be(newKitchen.HasPantry);
        kitchenData.Relationships.Should().BeNull();

        DataInFamilyHomeResponse familyHomeData2 = response.Atomic_results.ElementAt(2).Data.Should().BeOfType<DataInFamilyHomeResponse>().Subject;
        AttributesInFamilyHomeResponse familyHomeAttributes2 = familyHomeData2.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;
        familyHomeAttributes2.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes2.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes2.FloorCount.Should().Be(newFamilyHome.FloorCount);
        familyHomeData2.Relationships.Should().BeNull();

        DataInBedroomResponse bedroomData = response.Atomic_results.ElementAt(3).Data.Should().BeOfType<DataInBedroomResponse>().Subject;
        AttributesInBedroomResponse bedroomAttributes = bedroomData.Attributes.Should().BeOfType<AttributesInBedroomResponse>().Subject;
        bedroomAttributes.SurfaceInSquareMeters.Should().Be(newBedroom.SurfaceInSquareMeters);
        bedroomAttributes.BedCount.Should().Be(newBedroom.BedCount);
        bedroomData.Relationships.Should().BeNull();

        response.Atomic_results.ElementAt(4).Data.Should().BeNull();

        DataInFamilyHomeResponse familyHomeData5 = response.Atomic_results.ElementAt(5).Data.Should().BeOfType<DataInFamilyHomeResponse>().Subject;
        AttributesInFamilyHomeResponse familyHomeAttributes5 = familyHomeData5.Attributes.Should().BeOfType<AttributesInFamilyHomeResponse>().Subject;
        familyHomeAttributes5.SurfaceInSquareMeters.Should().Be(newFamilyHomeSurfaceInSquareMeters);
        familyHomeAttributes5.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes5.FloorCount.Should().Be(newFamilyHome.FloorCount);
        familyHomeData5.Relationships.Should().BeNull();

        response.Atomic_results.ElementAt(6).Data.Should().BeNull();

        long newMansionId = long.Parse(mansionData.Id.Should().NotBeNull().And.Subject);
        long newKitchenId = long.Parse(kitchenData.Id.Should().NotBeNull().And.Subject);
        long newFamilyHomeId = long.Parse(familyHomeData2.Id.Should().NotBeNull().And.Subject);
        long newBedroomId = long.Parse(bedroomData.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Mansion mansionInDatabase =
                await dbContext.Mansions.Include(mansion => mansion.Rooms).Include(mansion => mansion.Staff).FirstWithIdAsync(newMansionId);

            mansionInDatabase.SurfaceInSquareMeters.Should().Be(newMansion.SurfaceInSquareMeters);
            mansionInDatabase.NumberOfResidents.Should().Be(newMansion.NumberOfResidents);
            mansionInDatabase.OwnerName.Should().Be(newMansion.OwnerName);

            mansionInDatabase.Rooms.Should().BeEmpty();
            mansionInDatabase.Staff.Should().HaveCount(1);
            mansionInDatabase.Staff.ElementAt(0).Id.Should().Be(existingStaffMember.Id);

            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(newFamilyHomeId);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHomeSurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.Should().HaveCount(2);
            familyHomeInDatabase.Rooms.OfType<Kitchen>().Should().ContainSingle(kitchen => kitchen.Id == newKitchenId);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == newBedroomId);
        });
    }

    [Fact]
    public async Task Cannot_use_base_attributes_type_in_derived_data()
    {
        // Arrange
        int newFamilyHomeSurfaceInSquareMeters = _fakers.FamilyHome.GenerateOne().SurfaceInSquareMeters!.Value;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new SubsetOfOperationsInheritanceClient(httpClient);

        OperationsRequestDocument requestBody = new()
        {
            Atomic_operations =
            [
                new CreateResidenceOperation
                {
                    Data = new DataInCreateFamilyHomeRequest
                    {
                        Attributes = new AttributesInCreateResidenceRequest
                        {
                            SurfaceInSquareMeters = newFamilyHomeSurfaceInSquareMeters
                        }
                    }
                }
            ]
        };

        // Act
        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.PostOperationsAsync(requestBody));

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        exception.Result.Errors.Should().HaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("409");
        error.Title.Should().Be("Incompatible resource type found.");
        error.Detail.Should().Be("Expected openapi:discriminator with value 'familyHomes' instead of 'residences'.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/attributes/openapi:discriminator");
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
