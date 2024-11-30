using FluentAssertions;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfOperations.GeneratedCode;
using OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfOperations.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.ResourceInheritance;
using OpenApiTests.ResourceInheritance.Models;
using OpenApiTests.ResourceInheritance.SubsetOfOperations;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfOperations;

public sealed class SubsetOfOperationsInheritanceTests
    : IClassFixture<IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly ResourceInheritanceFakers _fakers = new();

    public SubsetOfOperationsInheritanceTests(IntegrationTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new SubsetOfOperationsInheritanceClient(requestAdapter);

        const string mansionLid = "mansion-lid";
        const string kitchenLid = "kitchen-lid";
        const string familyHomeLid = "family-home-lid";
        const string bedroomLid = "bedroom-lid";

        OperationsRequestDocument requestBody = new()
        {
            AtomicOperations =
            [
                // NOTE: CreateBuildingOperation is not generated, because it is turned off.
                new CreateResidenceOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateMansionRequest
                    {
                        Type = ResourceType.Mansions,
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
                                        Type = ResourceType.StaffMembers,
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
                    Op = AddOperationCode.Add,
                    // NOTE: DataInCreateRoomRequest is generated, but as abstract type.
                    Data = new DataInCreateKitchenRequest
                    {
                        Type = ResourceType.Kitchens,
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
                                    Type = ResourceType.Mansions,
                                    Lid = mansionLid
                                }
                            }
                        }
                    }
                },
                new CreateFamilyHomeOperation
                {
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateFamilyHomeRequest
                    {
                        Type = ResourceType.FamilyHomes,
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
                    Op = AddOperationCode.Add,
                    Data = new DataInCreateBedroomRequest
                    {
                        Type = ResourceType.Bedrooms,
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
                                    Type = ResourceType.FamilyHomes,
                                    Lid = familyHomeLid
                                }
                            }
                        }
                    }
                },
                new AddToFamilyHomeRoomsRelationshipOperation
                {
                    Op = AddOperationCode.Add,
                    Ref = new FamilyHomeRoomsRelationshipIdentifier
                    {
                        Type = FamilyHomeResourceType.FamilyHomes,
                        Relationship = FamilyHomeRoomsRelationshipName.Rooms,
                        Lid = familyHomeLid
                    },
                    Data =
                    [
                        new KitchenIdentifierInRequest
                        {
                            Type = ResourceType.Kitchens,
                            Lid = kitchenLid
                        }
                    ]
                },
                new UpdateResidenceOperation
                {
                    Op = UpdateOperationCode.Update,
                    // NOTE: Can use Ref to base type, while Data is derived.
                    Ref = new ResidenceIdentifierInRequest
                    {
                        Type = ResourceType.Residences,
                        Lid = familyHomeLid
                    },
                    Data = new DataInUpdateFamilyHomeRequest
                    {
                        Type = ResourceType.FamilyHomes,
                        Lid = familyHomeLid,
                        Attributes = new AttributesInUpdateResidenceRequest
                        {
                            SurfaceInSquareMeters = newFamilyHomeSurfaceInSquareMeters
                        }
                    }
                },
                new RemoveFromMansionStaffRelationshipOperation
                {
                    Op = RemoveOperationCode.Remove,
                    Ref = new MansionStaffRelationshipIdentifier
                    {
                        Type = MansionResourceType.Mansions,
                        Relationship = MansionStaffRelationshipName.Staff,
                        Lid = mansionLid
                    },
                    Data = []
                }
            ]
        };

        // Act
        OperationsResponseDocument? response = await apiClient.Operations.PostAsync(requestBody);

        // Assert
        response.ShouldNotBeNull();
        response.AtomicResults.ShouldHaveCount(7);

        MansionDataInResponse? mansionData = response.AtomicResults.ElementAt(0).Data.Should().BeOfType<MansionDataInResponse>().Subject;
        MansionAttributesInResponse? mansionAttributes = mansionData.Attributes.Should().BeOfType<MansionAttributesInResponse>().Subject;
        mansionAttributes.SurfaceInSquareMeters.Should().Be(newMansion.SurfaceInSquareMeters);
        mansionAttributes.NumberOfResidents.Should().Be(newMansion.NumberOfResidents);
        mansionAttributes.OwnerName.Should().Be(newMansion.OwnerName);
        mansionData.Relationships.Should().BeNull();

        KitchenDataInResponse? kitchenData = response.AtomicResults.ElementAt(1).Data.Should().BeOfType<KitchenDataInResponse>().Subject;
        KitchenAttributesInResponse? kitchenAttributes = kitchenData.Attributes.Should().BeOfType<KitchenAttributesInResponse>().Subject;
        kitchenAttributes.SurfaceInSquareMeters.Should().Be(newKitchen.SurfaceInSquareMeters);
        kitchenAttributes.HasPantry.Should().Be(newKitchen.HasPantry);
        kitchenData.Relationships.Should().BeNull();

        FamilyHomeDataInResponse? familyHomeData2 = response.AtomicResults.ElementAt(2).Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;
        FamilyHomeAttributesInResponse? familyHomeAttributes2 = familyHomeData2.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;
        familyHomeAttributes2.SurfaceInSquareMeters.Should().Be(newFamilyHome.SurfaceInSquareMeters);
        familyHomeAttributes2.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes2.FloorCount.Should().Be(newFamilyHome.FloorCount);
        familyHomeData2.Relationships.Should().BeNull();

        BedroomDataInResponse? bedroomData = response.AtomicResults.ElementAt(3).Data.Should().BeOfType<BedroomDataInResponse>().Subject;
        BedroomAttributesInResponse? bedroomAttributes = bedroomData.Attributes.Should().BeOfType<BedroomAttributesInResponse>().Subject;
        bedroomAttributes.SurfaceInSquareMeters.Should().Be(newBedroom.SurfaceInSquareMeters);
        bedroomAttributes.BedCount.Should().Be(newBedroom.BedCount);
        bedroomData.Relationships.Should().BeNull();

        response.AtomicResults.ElementAt(4).Data.Should().BeNull();

        FamilyHomeDataInResponse? familyHomeData5 = response.AtomicResults.ElementAt(5).Data.Should().BeOfType<FamilyHomeDataInResponse>().Subject;
        FamilyHomeAttributesInResponse? familyHomeAttributes5 = familyHomeData5.Attributes.Should().BeOfType<FamilyHomeAttributesInResponse>().Subject;
        familyHomeAttributes5.SurfaceInSquareMeters.Should().Be(newFamilyHomeSurfaceInSquareMeters);
        familyHomeAttributes5.NumberOfResidents.Should().Be(newFamilyHome.NumberOfResidents);
        familyHomeAttributes5.FloorCount.Should().Be(newFamilyHome.FloorCount);
        familyHomeData5.Relationships.Should().BeNull();

        response.AtomicResults.ElementAt(6).Data.Should().BeNull();

        long newMansionId = long.Parse(mansionData.Id.ShouldNotBeNull());
        long newKitchenId = long.Parse(kitchenData.Id.ShouldNotBeNull());
        long newFamilyHomeId = long.Parse(familyHomeData2.Id.ShouldNotBeNull());
        long newBedroomId = long.Parse(bedroomData.Id.ShouldNotBeNull());

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
        _requestAdapterFactory.Dispose();
    }
}
