using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable format

namespace OpenApiTests.ResourceInheritance.OnlyConcrete;

public sealed class OnlyConcreteInheritanceTests : ResourceInheritanceTests
{
    public OnlyConcreteInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
        : base(testContext, testOutputHelper, true, false)
    {
        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, OnlyConcreteEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, OnlyConcreteOperationFilter>();
        });
    }

    [Theory]
    [InlineData(typeof(District), JsonApiEndpoints.All)]
    [InlineData(typeof(StaffMember), JsonApiEndpoints.All)]
    [InlineData(typeof(Building), JsonApiEndpoints.None)]
    [InlineData(typeof(Residence), JsonApiEndpoints.All)]
    [InlineData(typeof(FamilyHome), JsonApiEndpoints.All)]
    [InlineData(typeof(Mansion), JsonApiEndpoints.All)]
    [InlineData(typeof(Room), JsonApiEndpoints.None)]
    [InlineData(typeof(Kitchen), JsonApiEndpoints.All)]
    [InlineData(typeof(Bedroom), JsonApiEndpoints.All)]
    [InlineData(typeof(Bathroom), JsonApiEndpoints.All)]
    [InlineData(typeof(LivingRoom), JsonApiEndpoints.All)]
    [InlineData(typeof(Toilet), JsonApiEndpoints.All)]
    [InlineData(typeof(Road), JsonApiEndpoints.All)]
    [InlineData(typeof(CyclePath), JsonApiEndpoints.All)]
    public override async Task Only_expected_endpoints_are_exposed(Type resourceClrType, JsonApiEndpoints expected)
    {
        await base.Only_expected_endpoints_are_exposed(resourceClrType, expected);
    }

    [Theory]
    [InlineData(true)]
    public override async Task Operations_endpoint_is_exposed(bool enabled)
    {
        await base.Operations_endpoint_is_exposed(enabled);
    }

    [Theory]
    [InlineData("resourceInCreateRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("resourceInUpdateRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("identifierInRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("resourceInResponse", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("dataInBuildingResponse", true, "familyHomes|mansions|residences")]
    [InlineData("buildingIdentifierInResponse", false, "familyHomes|mansions|residences")]
    [InlineData("dataInResidenceResponse", true, "familyHomes|mansions")]
    [InlineData("residenceIdentifierInResponse", true, "familyHomes|mansions")]
    [InlineData("dataInRoomResponse", true, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomIdentifierInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("dataInRoadResponse", true, "cyclePaths")]
    [InlineData("roadIdentifierInResponse", false, "cyclePaths")]
    public override async Task Expected_names_appear_in_type_discriminator_mapping(string schemaName, bool isWrapped, string? discriminatorValues)
    {
        await base.Expected_names_appear_in_type_discriminator_mapping(schemaName, isWrapped, discriminatorValues);
    }

    [Theory]
    [InlineData("attributesInCreateRequest",
        "familyHomes|mansions|residences|buildings|bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|cyclePaths|roads|districts|staffMembers")]
    [InlineData("attributesInUpdateRequest",
        "familyHomes|mansions|residences|buildings|bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|cyclePaths|roads|districts|staffMembers")]
    [InlineData("relationshipsInCreateRequest",
        "familyHomes|mansions|residences|buildings|bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|cyclePaths|roads|districts")]
    [InlineData("relationshipsInUpdateRequest",
        "familyHomes|mansions|residences|buildings|bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|cyclePaths|roads|districts")]
    [InlineData("!attributesInBuildingResponse", "familyHomes|mansions|residences")]
    [InlineData("!relationshipsInBuildingResponse", "familyHomes|mansions|residences")]
    [InlineData("!attributesInRoomResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("!relationshipsInRoomResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("!attributesInRoadResponse", "cyclePaths")]
    [InlineData("!relationshipsInRoadResponse", "cyclePaths")]
    [InlineData("atomicOperation",
        // @formatter:keep_existing_linebreaks true
        "createResidenceOperation|updateResidenceOperation|deleteResidenceOperation|addToResidenceRoomsRelationshipOperation|updateResidenceRoomsRelationshipOperation|removeFromResidenceRoomsRelationshipOperation|" +
        "createFamilyHomeOperation|updateFamilyHomeOperation|deleteFamilyHomeOperation|addToFamilyHomeRoomsRelationshipOperation|updateFamilyHomeRoomsRelationshipOperation|removeFromFamilyHomeRoomsRelationshipOperation|" +
        "createMansionOperation|updateMansionOperation|deleteMansionOperation|addToMansionRoomsRelationshipOperation|updateMansionRoomsRelationshipOperation|removeFromMansionRoomsRelationshipOperation|addToMansionStaffRelationshipOperation|updateMansionStaffRelationshipOperation|removeFromMansionStaffRelationshipOperation|" +
        "createBathroomOperation|updateBathroomOperation|deleteBathroomOperation|updateBathroomResidenceRelationshipOperation|" +
        "createBedroomOperation|updateBedroomOperation|deleteBedroomOperation|updateBedroomResidenceRelationshipOperation|" +
        "createKitchenOperation|updateKitchenOperation|deleteKitchenOperation|updateKitchenResidenceRelationshipOperation|" +
        "createLivingRoomOperation|updateLivingRoomOperation|deleteLivingRoomOperation|updateLivingRoomResidenceRelationshipOperation|" +
        "createToiletOperation|updateToiletOperation|deleteToiletOperation|updateToiletResidenceRelationshipOperation|" +
        "createDistrictOperation|updateDistrictOperation|deleteDistrictOperation|addToDistrictBuildingsRelationshipOperation|updateDistrictBuildingsRelationshipOperation|removeFromDistrictBuildingsRelationshipOperation|addToDistrictRoadsRelationshipOperation|updateDistrictRoadsRelationshipOperation|removeFromDistrictRoadsRelationshipOperation|" +
        "createRoadOperation|updateRoadOperation|deleteRoadOperation|" +
        "createCyclePathOperation|updateCyclePathOperation|deleteCyclePathOperation|" +
        "createStaffMemberOperation|updateStaffMemberOperation|deleteStaffMemberOperation"
        // @formatter:keep_existing_linebreaks restore
    )]
    public override async Task Expected_names_appear_in_openapi_discriminator_mapping(string schemaName, string? discriminatorValues)
    {
        await base.Expected_names_appear_in_openapi_discriminator_mapping(schemaName, discriminatorValues);
    }

    [Theory]
    [InlineData("buildingResourceType", "familyHomes|mansions|residences")]
    [InlineData("residenceResourceType", "familyHomes|mansions|residences")]
    [InlineData("familyHomeResourceType", null)]
    [InlineData("mansionResourceType", "mansions")]
    [InlineData("roomResourceType", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("bathroomResourceType", "bathrooms")]
    [InlineData("bedroomResourceType", "bedrooms")]
    [InlineData("kitchenResourceType", "kitchens")]
    [InlineData("livingRoomResourceType", "livingRooms")]
    [InlineData("toiletResourceType", "toilets")]
    [InlineData("roadResourceType", "roads|cyclePaths")]
    [InlineData("cyclePathResourceType", null)]
    [InlineData("districtResourceType", "districts")]
    [InlineData("staffMemberResourceType", "staffMembers")]
    [InlineData("resourceType",
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|roads|cyclePaths|districts|staffMembers")]
    public override async Task Expected_names_appear_in_resource_type_enum(string schemaName, string? enumValues)
    {
        await base.Expected_names_appear_in_resource_type_enum(schemaName, enumValues);
    }

    [Theory]
    [InlineData("resourceInCreateRequest", true, null, "type|meta")]
    [InlineData("attributesInCreateRequest", true, null, "openapi:discriminator")]
    [InlineData("relationshipsInCreateRequest", true, null, "openapi:discriminator")]
    [InlineData("resourceInUpdateRequest", true, null, "type|meta")]
    [InlineData("attributesInUpdateRequest", true, null, "openapi:discriminator")]
    [InlineData("relationshipsInUpdateRequest", true, null, "openapi:discriminator")]
    [InlineData("identifierInRequest", true, null, "type|meta")]
    [InlineData("resourceInResponse", true, null, "type|meta")]
    [InlineData("atomicOperation", true, null, "openapi:discriminator|meta")]
    // Building hierarchy: Resource Data
    [InlineData("dataInCreateBuildingRequest", true, "resourceInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateResidenceRequest", false, "dataInCreateBuildingRequest", null)]
    [InlineData("dataInCreateFamilyHomeRequest", false, "dataInCreateResidenceRequest", null)]
    [InlineData("dataInCreateMansionRequest", false, "dataInCreateResidenceRequest", null)]
    [InlineData("dataInUpdateBuildingRequest", true, "resourceInUpdateRequest", "id|lid|attributes|relationships")]
    [InlineData("dataInUpdateResidenceRequest", false, "dataInUpdateBuildingRequest", null)]
    [InlineData("dataInUpdateFamilyHomeRequest", false, "dataInUpdateResidenceRequest", null)]
    [InlineData("dataInUpdateMansionRequest", false, "dataInUpdateResidenceRequest", null)]
    [InlineData("dataInBuildingResponse", true, "resourceInResponse", "id|attributes|relationships|links")]
    [InlineData("dataInResidenceResponse", false, "dataInBuildingResponse", null)]
    [InlineData("dataInFamilyHomeResponse", false, "dataInResidenceResponse", null)]
    [InlineData("dataInMansionResponse", false, "dataInResidenceResponse", null)]
    // Building hierarchy: Attributes
    [InlineData("attributesInCreateBuildingRequest", true, "attributesInCreateRequest", "surfaceInSquareMeters")]
    [InlineData("attributesInCreateResidenceRequest", false, "attributesInCreateBuildingRequest", "numberOfResidents")]
    [InlineData("attributesInCreateFamilyHomeRequest", false, "attributesInCreateResidenceRequest", "floorCount")]
    [InlineData("attributesInCreateMansionRequest", false, "attributesInCreateResidenceRequest", "ownerName")]
    [InlineData("attributesInUpdateBuildingRequest", true, "attributesInUpdateRequest", "surfaceInSquareMeters")]
    [InlineData("attributesInUpdateResidenceRequest", false, "attributesInUpdateBuildingRequest", "numberOfResidents")]
    [InlineData("attributesInUpdateFamilyHomeRequest", false, "attributesInUpdateResidenceRequest", "floorCount")]
    [InlineData("attributesInUpdateMansionRequest", false, "attributesInUpdateResidenceRequest", "ownerName")]
    [InlineData("attributesInBuildingResponse", true, "attributesInResponse", "surfaceInSquareMeters")]
    [InlineData("attributesInResidenceResponse", false, "attributesInBuildingResponse", "numberOfResidents")]
    [InlineData("attributesInFamilyHomeResponse", false, "attributesInResidenceResponse", "floorCount")]
    [InlineData("attributesInMansionResponse", false, "attributesInResidenceResponse", "ownerName")]
    // Building hierarchy: Relationships
    [InlineData("relationshipsInCreateBuildingRequest", true, "relationshipsInCreateRequest", null)]
    [InlineData("relationshipsInCreateResidenceRequest", false, "relationshipsInCreateBuildingRequest", "rooms")]
    [InlineData("relationshipsInCreateFamilyHomeRequest", false, "relationshipsInCreateResidenceRequest", null)]
    [InlineData("relationshipsInCreateMansionRequest", false, "relationshipsInCreateResidenceRequest", "staff")]
    [InlineData("relationshipsInUpdateBuildingRequest", true, "relationshipsInUpdateRequest", null)]
    [InlineData("relationshipsInUpdateResidenceRequest", false, "relationshipsInUpdateBuildingRequest", "rooms")]
    [InlineData("relationshipsInUpdateFamilyHomeRequest", false, "relationshipsInUpdateResidenceRequest", null)]
    [InlineData("relationshipsInUpdateMansionRequest", false, "relationshipsInUpdateResidenceRequest", "staff")]
    [InlineData("relationshipsInBuildingResponse", true, "relationshipsInResponse", null)]
    [InlineData("relationshipsInResidenceResponse", false, "relationshipsInBuildingResponse", "rooms")]
    [InlineData("relationshipsInFamilyHomeResponse", false, "relationshipsInResidenceResponse", null)]
    [InlineData("relationshipsInMansionResponse", false, "relationshipsInResidenceResponse", "staff")]
    // Building hierarchy: Resource Identifiers
    [InlineData("buildingIdentifierInRequest", false, "identifierInRequest", "id|lid")]
    [InlineData("residenceIdentifierInRequest", false, "buildingIdentifierInRequest", null)]
    [InlineData("familyHomeIdentifierInRequest", false, "residenceIdentifierInRequest", null)]
    [InlineData("mansionIdentifierInRequest", false, "residenceIdentifierInRequest", null)]
    [InlineData("buildingIdentifierInResponse", true, null, "type|id|meta")]
    [InlineData("residenceIdentifierInResponse", false, "buildingIdentifierInResponse", null)]
    [InlineData("familyHomeIdentifierInResponse", false, "residenceIdentifierInResponse", null)]
    [InlineData("mansionIdentifierInResponse", false, "residenceIdentifierInResponse", null)]
    // Building hierarchy: Atomic Operations
    [InlineData("createBuildingOperation", false, null, null)]
    [InlineData("createResidenceOperation", false, "atomicOperation", "op|data")]
    [InlineData("createFamilyHomeOperation", false, "createResidenceOperation", null)]
    [InlineData("createMansionOperation", false, "createResidenceOperation", null)]
    [InlineData("updateBuildingOperation", false, null, null)]
    [InlineData("updateResidenceOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateFamilyHomeOperation", false, "updateResidenceOperation", null)]
    [InlineData("updateMansionOperation", false, "updateResidenceOperation", null)]
    [InlineData("deleteBuildingOperation", false, null, null)]
    [InlineData("deleteResidenceOperation", false, "atomicOperation", "op|ref")]
    [InlineData("deleteFamilyHomeOperation", false, "deleteResidenceOperation", null)]
    [InlineData("deleteMansionOperation", false, "deleteResidenceOperation", null)]
    [InlineData("updateResidenceRoomsRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateFamilyHomeRoomsRelationshipOperation", false, "updateResidenceRoomsRelationshipOperation", null)]
    [InlineData("updateMansionRoomsRelationshipOperation", false, "updateResidenceRoomsRelationshipOperation", null)]
    [InlineData("updateMansionStaffRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("addToResidenceRoomsRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("addToFamilyHomeRoomsRelationshipOperation", false, "addToResidenceRoomsRelationshipOperation", null)]
    [InlineData("addToMansionRoomsRelationshipOperation", false, "addToResidenceRoomsRelationshipOperation", null)]
    [InlineData("addToMansionStaffRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("removeFromResidenceRoomsRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("removeFromFamilyHomeRoomsRelationshipOperation", false, "removeFromResidenceRoomsRelationshipOperation", null)]
    [InlineData("removeFromMansionRoomsRelationshipOperation", false, "removeFromResidenceRoomsRelationshipOperation", null)]
    [InlineData("removeFromMansionStaffRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    // Room hierarchy: Resource Data
    [InlineData("dataInCreateRoomRequest", true, "resourceInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateBathroomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateBedroomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateKitchenRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateLivingRoomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateToiletRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInUpdateRoomRequest", true, "resourceInUpdateRequest", "id|lid|attributes|relationships")]
    [InlineData("dataInUpdateBathroomRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateBedroomRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateKitchenRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateLivingRoomRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateToiletRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInRoomResponse", true, "resourceInResponse", "id|attributes|relationships|links")]
    [InlineData("dataInBathroomResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInBedroomResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInKitchenResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInLivingRoomResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInToiletResponse", false, "dataInRoomResponse", null)]
    // Room hierarchy: Attributes
    [InlineData("attributesInCreateRoomRequest", true, "attributesInCreateRequest", "surfaceInSquareMeters")]
    [InlineData("attributesInCreateBathroomRequest", false, "attributesInCreateRoomRequest", "hasBath")]
    [InlineData("attributesInCreateBedroomRequest", false, "attributesInCreateRoomRequest", "bedCount")]
    [InlineData("attributesInCreateKitchenRequest", false, "attributesInCreateRoomRequest", "hasPantry")]
    [InlineData("attributesInCreateLivingRoomRequest", false, "attributesInCreateRoomRequest", "hasDiningTable")]
    [InlineData("attributesInCreateToiletRequest", false, "attributesInCreateRoomRequest", "hasSink")]
    [InlineData("attributesInUpdateRoomRequest", true, "attributesInUpdateRequest", "surfaceInSquareMeters")]
    [InlineData("attributesInUpdateBathroomRequest", false, "attributesInUpdateRoomRequest", "hasBath")]
    [InlineData("attributesInUpdateBedroomRequest", false, "attributesInUpdateRoomRequest", "bedCount")]
    [InlineData("attributesInUpdateKitchenRequest", false, "attributesInUpdateRoomRequest", "hasPantry")]
    [InlineData("attributesInUpdateLivingRoomRequest", false, "attributesInUpdateRoomRequest", "hasDiningTable")]
    [InlineData("attributesInUpdateToiletRequest", false, "attributesInUpdateRoomRequest", "hasSink")]
    [InlineData("attributesInRoomResponse", true, "attributesInResponse", "surfaceInSquareMeters")]
    [InlineData("attributesInBathroomResponse", false, "attributesInRoomResponse", "hasBath")]
    [InlineData("attributesInBedroomResponse", false, "attributesInRoomResponse", "bedCount")]
    [InlineData("attributesInKitchenResponse", false, "attributesInRoomResponse", "hasPantry")]
    [InlineData("attributesInLivingRoomResponse", false, "attributesInRoomResponse", "hasDiningTable")]
    [InlineData("attributesInToiletResponse", false, "attributesInRoomResponse", "hasSink")]
    // Room hierarchy: Relationships
    [InlineData("relationshipsInCreateRoomRequest", true, "relationshipsInCreateRequest", "residence")]
    [InlineData("relationshipsInCreateBathroomRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateBedroomRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateKitchenRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateLivingRoomRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateToiletRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInUpdateRoomRequest", true, "relationshipsInUpdateRequest", "residence")]
    [InlineData("relationshipsInUpdateBathroomRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateBedroomRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateKitchenRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateLivingRoomRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateToiletRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInRoomResponse", true, "relationshipsInResponse", "residence")]
    [InlineData("relationshipsInBathroomResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInBedroomResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInKitchenResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInLivingRoomResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInToiletResponse", false, "relationshipsInRoomResponse", null)]
    // Room hierarchy: Resource Identifiers
    [InlineData("roomIdentifierInRequest", false, "identifierInRequest", "id|lid")]
    [InlineData("bathroomIdentifierInRequest", false, "roomIdentifierInRequest", null)]
    [InlineData("bedroomIdentifierInRequest", false, "roomIdentifierInRequest", null)]
    [InlineData("kitchenIdentifierInRequest", false, "roomIdentifierInRequest", null)]
    [InlineData("livingRoomIdentifierInRequest", false, "roomIdentifierInRequest", null)]
    [InlineData("toiletIdentifierInRequest", false, "roomIdentifierInRequest", null)]
    [InlineData("roomIdentifierInResponse", true, null, "type|id|meta")]
    [InlineData("bathroomIdentifierInResponse", false, "roomIdentifierInResponse", null)]
    [InlineData("bedroomIdentifierInResponse", false, "roomIdentifierInResponse", null)]
    [InlineData("kitchenIdentifierInResponse", false, "roomIdentifierInResponse", null)]
    [InlineData("livingRoomIdentifierInResponse", false, "roomIdentifierInResponse", null)]
    [InlineData("toiletIdentifierInResponse", false, "roomIdentifierInResponse", null)]
    // Room hierarchy: Atomic Operations
    [InlineData("createRoomOperation", false, null, null)]
    [InlineData("createBathroomOperation", false, "atomicOperation", "op|data")]
    [InlineData("createBedroomOperation", false, "atomicOperation", "op|data")]
    [InlineData("createKitchenOperation", false, "atomicOperation", "op|data")]
    [InlineData("createLivingRoomOperation", false, "atomicOperation", "op|data")]
    [InlineData("createToiletOperation", false, "atomicOperation", "op|data")]
    [InlineData("updateRoomOperation", false, null, null)]
    [InlineData("updateBathroomOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateBedroomOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateKitchenOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateLivingRoomOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateToiletOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("deleteRoomOperation", false, null, null)]
    [InlineData("deleteBathroomOperation", false, "atomicOperation", "op|ref")]
    [InlineData("deleteBedroomOperation", false, "atomicOperation", "op|ref")]
    [InlineData("deleteKitchenOperation", false, "atomicOperation", "op|ref")]
    [InlineData("deleteLivingRoomOperation", false, "atomicOperation", "op|ref")]
    [InlineData("deleteToiletOperation", false, "atomicOperation", "op|ref")]
    [InlineData("updateRoomResidenceRelationshipOperation", false, null, null)]
    [InlineData("updateBathroomResidenceRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateBedroomResidenceRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateKitchenResidenceRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateLivingRoomResidenceRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateToiletResidenceRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    // Road hierarchy: Resource Data
    [InlineData("dataInCreateRoadRequest", false, "resourceInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateCyclePathRequest", false, "dataInCreateRoadRequest", null)]
    [InlineData("dataInUpdateRoadRequest", false, "resourceInUpdateRequest", "id|lid|attributes|relationships")]
    [InlineData("dataInUpdateCyclePathRequest", false, "dataInUpdateRoadRequest", null)]
    [InlineData("dataInRoadResponse", false, "resourceInResponse", "id|attributes|relationships|links")]
    [InlineData("dataInCyclePathResponse", false, "dataInRoadResponse", null)]
    // Road hierarchy: Attributes
    [InlineData("attributesInCreateRoadRequest", false, "attributesInCreateRequest", "lengthInMeters")]
    [InlineData("attributesInCreateCyclePathRequest", false, "attributesInCreateRoadRequest", "hasLaneForPedestrians")]
    [InlineData("attributesInUpdateRoadRequest", false, "attributesInUpdateRequest", "lengthInMeters")]
    [InlineData("attributesInUpdateCyclePathRequest", false, "attributesInUpdateRoadRequest", "hasLaneForPedestrians")]
    [InlineData("attributesInRoadResponse", false, "attributesInResponse", "lengthInMeters")]
    [InlineData("attributesInCyclePathResponse", false, "attributesInRoadResponse", "hasLaneForPedestrians")]
    // Road hierarchy: Relationships
    [InlineData("relationshipsInCreateRoadRequest", false, "relationshipsInCreateRequest", null)]
    [InlineData("relationshipsInCreateCyclePathRequest", false, "relationshipsInCreateRoadRequest", null)]
    [InlineData("relationshipsInUpdateRoadRequest", false, "relationshipsInUpdateRequest", null)]
    [InlineData("relationshipsInUpdateCyclePathRequest", false, "relationshipsInUpdateRoadRequest", null)]
    [InlineData("relationshipsInRoadResponse", false, "relationshipsInResponse", null)]
    [InlineData("relationshipsInCyclePathResponse", false, "relationshipsInRoadResponse", null)]
    // Road hierarchy: Resource Identifiers
    [InlineData("roadIdentifierInRequest", false, "identifierInRequest", "id|lid")]
    [InlineData("cyclePathIdentifierInRequest", false, "roadIdentifierInRequest", null)]
    [InlineData("roadIdentifierInResponse", false, null, "type|id|meta")]
    [InlineData("cyclePathIdentifierInResponse", false, "roadIdentifierInResponse", null)]
    // Road hierarchy: Atomic Operations
    [InlineData("createRoadOperation", false, "atomicOperation", "op|data")]
    [InlineData("createCyclePathOperation", false, "createRoadOperation", null)]
    [InlineData("updateRoadOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateCyclePathOperation", false, "updateRoadOperation", null)]
    [InlineData("deleteRoadOperation", false, "atomicOperation", "op|ref")]
    [InlineData("deleteCyclePathOperation", false, "deleteRoadOperation", null)]
    public override async Task Component_schemas_have_expected_base_type(string schemaName, bool isAbstract, string? baseType, string? properties)
    {
        await base.Component_schemas_have_expected_base_type(schemaName, isAbstract, baseType, properties);
    }
}
