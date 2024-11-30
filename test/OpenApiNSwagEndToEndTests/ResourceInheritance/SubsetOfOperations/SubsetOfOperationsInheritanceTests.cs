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

public sealed class SubsetOfOperationsInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly ResourceInheritanceFakers _fakers = new();

    public SubsetOfOperationsInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
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
    public async Task Test2()
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
                            FloorCount = newFamilyHome.FloorCount
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
                        Attributes = new AttributesInUpdateResidenceRequest
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
        response.ShouldNotBeNull();
        response.Atomic_results.ShouldHaveCount(7);

        MansionDataInResponse? mansionData = response.Atomic_results.ElementAt(0).Data.Should().BeOfType<MansionDataInResponse>().Subject;
        MansionAttributesInResponse? mansionAttributes = mansionData.Attributes.Should().BeOfType<MansionAttributesInResponse>().Subject;
        mansionAttributes.SurfaceInSquareMeters.Should().Be(newMansion.SurfaceInSquareMeters);
        mansionAttributes.NumberOfResidents.Should().Be(newMansion.NumberOfResidents);
        mansionAttributes.OwnerName.Should().Be(newMansion.OwnerName);
        mansionData.Relationships.Should().BeNull();

        KitchenDataInResponse? kitchenData = response.Atomic_results.ElementAt(1).Data.Should().BeOfType<KitchenDataInResponse>().Subject;
        KitchenAttributesInResponse? kitchenAttributes = kitchenData.Attributes.Should().BeOfType<KitchenAttributesInResponse>().Subject;
        kitchenAttributes.SurfaceInSquareMeters.Should().Be(newKitchen.SurfaceInSquareMeters);
        kitchenAttributes.HasPantry.Should().Be(newKitchen.HasPantry);
        kitchenData.Relationships.Should().BeNull();

        FamilyHomeDataInResponse? familyHomeData2 = response.Atomic_results.ElementAt(2).Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;
        FamilyHomeAttributesInResponse? familyHomeAttributes2 = familyHomeData2.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;
        familyHomeAttributes2.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes2.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes2.FloorCount.Should().Be(newFamilyHome.FloorCount);
        familyHomeData2.Relationships.Should().BeNull();

        BedroomDataInResponse? bedroomData = response.Atomic_results.ElementAt(3).Data.Should().BeOfType<BedroomDataInResponse>().Subject;
        BedroomAttributesInResponse? bedroomAttributes = bedroomData.Attributes.Should().BeOfType<BedroomAttributesInResponse>().Subject;
        bedroomAttributes.SurfaceInSquareMeters.Should().Be(newBedroom.SurfaceInSquareMeters);
        bedroomAttributes.BedCount.Should().Be(newBedroom.BedCount);
        bedroomData.Relationships.Should().BeNull();

        response.Atomic_results.ElementAt(4).Data.Should().BeNull();

        FamilyHomeDataInResponse? familyHomeData5 = response.Atomic_results.ElementAt(5).Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;
        FamilyHomeAttributesInResponse? familyHomeAttributes5 = familyHomeData5.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;
        familyHomeAttributes5.SurfaceInSquareMeters.Should().Be(newFamilyHomeSurfaceInSquareMeters);
        familyHomeAttributes5.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes5.FloorCount.Should().Be(newFamilyHome.FloorCount);
        familyHomeData5.Relationships.Should().BeNull();

        response.Atomic_results.ElementAt(6).Data.Should().BeNull();

        long newMansionId = long.Parse(mansionData.Id);
        long newKitchenId = long.Parse(kitchenData.Id);
        long newFamilyHomeId = long.Parse(familyHomeData2.Id);
        long newBedroomId = long.Parse(bedroomData.Id);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Mansion mansionInDatabase =
                await dbContext.Mansions.Include(mansion => mansion.Rooms).Include(mansion => mansion.Staff).FirstWithIdAsync(newMansionId);

            mansionInDatabase.SurfaceInSquareMeters.Should().Be(newMansion.SurfaceInSquareMeters);
            mansionInDatabase.NumberOfResidents.Should().Be(newMansion.NumberOfResidents);
            mansionInDatabase.OwnerName.Should().Be(newMansion.OwnerName);

            mansionInDatabase.Rooms.Should().BeEmpty();
            mansionInDatabase.Staff.ShouldHaveCount(1);
            mansionInDatabase.Staff.ElementAt(0).Id.Should().Be(existingStaffMember.Id);

            FamilyHome familyHomeInDatabase = await dbContext.FamilyHomes.Include(familyHome => familyHome.Rooms).FirstWithIdAsync(newFamilyHomeId);

            familyHomeInDatabase.SurfaceInSquareMeters.Should().Be(newFamilyHomeSurfaceInSquareMeters);
            familyHomeInDatabase.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
            familyHomeInDatabase.FloorCount.Should().Be(newFamilyHome.FloorCount);

            familyHomeInDatabase.Rooms.ShouldHaveCount(2);
            familyHomeInDatabase.Rooms.OfType<Kitchen>().Should().ContainSingle(kitchen => kitchen.Id == newKitchenId);
            familyHomeInDatabase.Rooms.OfType<Bedroom>().Should().ContainSingle(bedroom => bedroom.Id == newBedroomId);
        });
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
