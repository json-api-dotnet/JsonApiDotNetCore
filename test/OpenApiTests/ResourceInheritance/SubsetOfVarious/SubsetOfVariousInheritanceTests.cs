using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.ResourceInheritance.SubsetOfVarious;

public sealed class SubsetOfVariousInheritanceTests : ResourceInheritanceTests
{
    public SubsetOfVariousInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext,
        ITestOutputHelper testOutputHelper)
        : base(testContext, testOutputHelper, false, true)
    {
        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, SubsetOfVariousEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, SubsetOfVariousOperationFilter>();
        });
    }

    [Theory]
    [InlineData(typeof(District), JsonApiEndpoints.GetCollection)]
    [InlineData(typeof(StaffMember), JsonApiEndpoints.None)]
    [InlineData(typeof(Building), JsonApiEndpoints.Post | JsonApiEndpoints.Patch)]
    [InlineData(typeof(Residence), JsonApiEndpoints.None)]
    [InlineData(typeof(FamilyHome), JsonApiEndpoints.GetRelationship)]
    [InlineData(typeof(Mansion), JsonApiEndpoints.None)]
    [InlineData(typeof(Room), JsonApiEndpoints.None)]
    [InlineData(typeof(Kitchen), JsonApiEndpoints.None)]
    [InlineData(typeof(Bedroom), JsonApiEndpoints.None)]
    [InlineData(typeof(Bathroom), JsonApiEndpoints.None)]
    [InlineData(typeof(LivingRoom), JsonApiEndpoints.None)]
    [InlineData(typeof(Toilet), JsonApiEndpoints.None)]
    [InlineData(typeof(Road), JsonApiEndpoints.None)]
    [InlineData(typeof(CyclePath), JsonApiEndpoints.GetSingle)]
    public override async Task Only_expected_endpoints_are_exposed(Type resourceClrType, JsonApiEndpoints expected)
    {
        await base.Only_expected_endpoints_are_exposed(resourceClrType, expected);
    }

    [Theory]
    [InlineData(false)]
    public override async Task Operations_endpoint_is_exposed(bool enabled)
    {
        await base.Operations_endpoint_is_exposed(enabled);
    }

    [Theory]
    [InlineData("resourceInCreateRequest", false, "familyHomes|mansions|residences|buildings")]
    [InlineData("resourceInUpdateRequest", false, "familyHomes|mansions|residences|buildings")]
    [InlineData("identifierInRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|staffMembers")]
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
    [InlineData("attributesInCreateRequest", "familyHomes|mansions|residences|buildings")]
    [InlineData("attributesInUpdateRequest", "familyHomes|mansions|residences|buildings")]
    [InlineData("relationshipsInCreateRequest", "familyHomes|mansions|residences|buildings")]
    [InlineData("relationshipsInUpdateRequest", "familyHomes|mansions|residences|buildings")]
    [InlineData("!attributesInBuildingResponse", "familyHomes|mansions|residences")]
    [InlineData("!relationshipsInBuildingResponse", "familyHomes|mansions|residences")]
    [InlineData("!attributesInRoomResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("!relationshipsInRoomResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("!attributesInRoadResponse", "cyclePaths")]
    [InlineData("!relationshipsInRoadResponse", "cyclePaths")]
    [InlineData("atomicOperation", null)]
    public override async Task Expected_names_appear_in_openapi_discriminator_mapping(string schemaName, string? discriminatorValues)
    {
        await base.Expected_names_appear_in_openapi_discriminator_mapping(schemaName, discriminatorValues);
    }

    [Theory]
    [InlineData("buildingResourceType", "familyHomes|mansions|residences")]
    [InlineData("residenceResourceType", null)]
    [InlineData("familyHomeResourceType", null)]
    [InlineData("mansionResourceType", null)]
    [InlineData("roomResourceType", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("bathroomResourceType", null)]
    [InlineData("bedroomResourceType", null)]
    [InlineData("kitchenResourceType", null)]
    [InlineData("livingRoomResourceType", null)]
    [InlineData("toiletResourceType", null)]
    [InlineData("roadResourceType", "roads|cyclePaths")]
    [InlineData("cyclePathResourceType", null)]
    [InlineData("districtResourceType", null)]
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
    [InlineData("atomicOperation", true, null, null)]
    // Building hierarchy: Resource Data
    [InlineData("dataInCreateBuildingRequest", true, "resourceInCreateRequest", "attributes|relationships")]
    [InlineData("dataInCreateResidenceRequest", false, "dataInCreateBuildingRequest", null)]
    [InlineData("dataInCreateFamilyHomeRequest", false, "dataInCreateResidenceRequest", null)]
    [InlineData("dataInCreateMansionRequest", false, "dataInCreateResidenceRequest", null)]
    [InlineData("dataInUpdateBuildingRequest", true, "resourceInUpdateRequest", "id|attributes|relationships")]
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
    [InlineData("buildingIdentifierInRequest", false, null, null)]
    [InlineData("residenceIdentifierInRequest", false, null, null)]
    [InlineData("familyHomeIdentifierInRequest", false, null, null)]
    [InlineData("mansionIdentifierInRequest", false, null, null)]
    [InlineData("buildingIdentifierInResponse", true, null, "type|id|meta")]
    [InlineData("residenceIdentifierInResponse", false, "buildingIdentifierInResponse", null)]
    [InlineData("familyHomeIdentifierInResponse", false, "residenceIdentifierInResponse", null)]
    [InlineData("mansionIdentifierInResponse", false, "residenceIdentifierInResponse", null)]
    // Building hierarchy: Atomic Operations
    [InlineData("createBuildingOperation", false, null, null)]
    [InlineData("createResidenceOperation", false, null, null)]
    [InlineData("createFamilyHomeOperation", false, null, null)]
    [InlineData("createMansionOperation", false, null, null)]
    [InlineData("updateBuildingOperation", false, null, null)]
    [InlineData("updateResidenceOperation", false, null, null)]
    [InlineData("updateFamilyHomeOperation", false, null, null)]
    [InlineData("updateMansionOperation", false, null, null)]
    [InlineData("deleteBuildingOperation", false, null, null)]
    [InlineData("deleteResidenceOperation", false, null, null)]
    [InlineData("deleteFamilyHomeOperation", false, null, null)]
    [InlineData("deleteMansionOperation", false, null, null)]
    [InlineData("updateResidenceRoomsRelationshipOperation", false, null, null)]
    [InlineData("updateFamilyHomeRoomsRelationshipOperation", false, null, null)]
    [InlineData("updateMansionRoomsRelationshipOperation", false, null, null)]
    [InlineData("updateMansionStaffRelationshipOperation", false, null, null)]
    [InlineData("addToResidenceRoomsRelationshipOperation", false, null, null)]
    [InlineData("addToFamilyHomeRoomsRelationshipOperation", false, null, null)]
    [InlineData("addToMansionRoomsRelationshipOperation", false, null, null)]
    [InlineData("addToMansionStaffRelationshipOperation", false, null, null)]
    [InlineData("removeFromResidenceRoomsRelationshipOperation", false, null, null)]
    [InlineData("removeFromFamilyHomeRoomsRelationshipOperation", false, null, null)]
    [InlineData("removeFromMansionRoomsRelationshipOperation", false, null, null)]
    [InlineData("removeFromMansionStaffRelationshipOperation", false, null, null)]
    // Room hierarchy: Resource Data
    [InlineData("dataInCreateRoomRequest", true, null, null)]
    [InlineData("dataInCreateBathroomRequest", false, null, null)]
    [InlineData("dataInCreateBedroomRequest", false, null, null)]
    [InlineData("dataInCreateKitchenRequest", false, null, null)]
    [InlineData("dataInCreateLivingRoomRequest", false, null, null)]
    [InlineData("dataInCreateToiletRequest", false, null, null)]
    [InlineData("dataInUpdateRoomRequest", true, null, null)]
    [InlineData("dataInUpdateBathroomRequest", false, null, null)]
    [InlineData("dataInUpdateBedroomRequest", false, null, null)]
    [InlineData("dataInUpdateKitchenRequest", false, null, null)]
    [InlineData("dataInUpdateLivingRoomRequest", false, null, null)]
    [InlineData("dataInUpdateToiletRequest", false, null, null)]
    [InlineData("dataInRoomResponse", true, "resourceInResponse", "id|attributes|relationships|links")]
    [InlineData("dataInBathroomResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInBedroomResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInKitchenResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInLivingRoomResponse", false, "dataInRoomResponse", null)]
    [InlineData("dataInToiletResponse", false, "dataInRoomResponse", null)]
    // Room hierarchy: Attributes
    [InlineData("attributesInCreateRoomRequest", true, null, null)]
    [InlineData("attributesInCreateBathroomRequest", false, null, null)]
    [InlineData("attributesInCreateBedroomRequest", false, null, null)]
    [InlineData("attributesInCreateKitchenRequest", false, null, null)]
    [InlineData("attributesInCreateLivingRoomRequest", false, null, null)]
    [InlineData("attributesInCreateToiletRequest", false, null, null)]
    [InlineData("attributesInUpdateRoomRequest", true, null, null)]
    [InlineData("attributesInUpdateBathroomRequest", false, null, null)]
    [InlineData("attributesInUpdateBedroomRequest", false, null, null)]
    [InlineData("attributesInUpdateKitchenRequest", false, null, null)]
    [InlineData("attributesInUpdateLivingRoomRequest", false, null, null)]
    [InlineData("attributesInUpdateToiletRequest", false, null, null)]
    [InlineData("attributesInRoomResponse", true, "attributesInResponse", "surfaceInSquareMeters")]
    [InlineData("attributesInBathroomResponse", false, "attributesInRoomResponse", "hasBath")]
    [InlineData("attributesInBedroomResponse", false, "attributesInRoomResponse", "bedCount")]
    [InlineData("attributesInKitchenResponse", false, "attributesInRoomResponse", "hasPantry")]
    [InlineData("attributesInLivingRoomResponse", false, "attributesInRoomResponse", "hasDiningTable")]
    [InlineData("attributesInToiletResponse", false, "attributesInRoomResponse", "hasSink")]
    // Room hierarchy: Relationships
    [InlineData("relationshipsInCreateRoomRequest", true, null, null)]
    [InlineData("relationshipsInCreateBathroomRequest", false, null, null)]
    [InlineData("relationshipsInCreateBedroomRequest", false, null, null)]
    [InlineData("relationshipsInCreateKitchenRequest", false, null, null)]
    [InlineData("relationshipsInCreateLivingRoomRequest", false, null, null)]
    [InlineData("relationshipsInCreateToiletRequest", false, null, null)]
    [InlineData("relationshipsInUpdateRoomRequest", true, null, null)]
    [InlineData("relationshipsInUpdateBathroomRequest", false, null, null)]
    [InlineData("relationshipsInUpdateBedroomRequest", false, null, null)]
    [InlineData("relationshipsInUpdateKitchenRequest", false, null, null)]
    [InlineData("relationshipsInUpdateLivingRoomRequest", false, null, null)]
    [InlineData("relationshipsInUpdateToiletRequest", false, null, null)]
    [InlineData("relationshipsInRoomResponse", true, "relationshipsInResponse", "residence")]
    [InlineData("relationshipsInBathroomResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInBedroomResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInKitchenResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInLivingRoomResponse", false, "relationshipsInRoomResponse", null)]
    [InlineData("relationshipsInToiletResponse", false, "relationshipsInRoomResponse", null)]
    // Room hierarchy: Resource Identifiers
    [InlineData("roomIdentifierInRequest", false, "identifierInRequest", "id")]
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
    [InlineData("createBathroomOperation", false, null, null)]
    [InlineData("createBedroomOperation", false, null, null)]
    [InlineData("createKitchenOperation", false, null, null)]
    [InlineData("createLivingRoomOperation", false, null, null)]
    [InlineData("createToiletOperation", false, null, null)]
    [InlineData("updateRoomOperation", false, null, null)]
    [InlineData("updateBathroomOperation", false, null, null)]
    [InlineData("updateBedroomOperation", false, null, null)]
    [InlineData("updateKitchenOperation", false, null, null)]
    [InlineData("updateLivingRoomOperation", false, null, null)]
    [InlineData("updateToiletOperation", false, null, null)]
    [InlineData("deleteRoomOperation", false, null, null)]
    [InlineData("deleteBathroomOperation", false, null, null)]
    [InlineData("deleteBedroomOperation", false, null, null)]
    [InlineData("deleteKitchenOperation", false, null, null)]
    [InlineData("deleteLivingRoomOperation", false, null, null)]
    [InlineData("deleteToiletOperation", false, null, null)]
    [InlineData("updateRoomResidenceRelationshipOperation", false, null, null)]
    [InlineData("updateBathroomResidenceRelationshipOperation", false, null, null)]
    [InlineData("updateBedroomResidenceRelationshipOperation", false, null, null)]
    [InlineData("updateKitchenResidenceRelationshipOperation", false, null, null)]
    [InlineData("updateLivingRoomResidenceRelationshipOperation", false, null, null)]
    [InlineData("updateToiletResidenceRelationshipOperation", false, null, null)]
    // Road hierarchy: Resource Data
    [InlineData("dataInCreateRoadRequest", false, null, null)]
    [InlineData("dataInCreateCyclePathRequest", false, null, null)]
    [InlineData("dataInUpdateRoadRequest", false, null, null)]
    [InlineData("dataInUpdateCyclePathRequest", false, null, null)]
    [InlineData("dataInRoadResponse", false, "resourceInResponse", "id|attributes|relationships|links")]
    [InlineData("dataInCyclePathResponse", false, "dataInRoadResponse", null)]
    // Road hierarchy: Attributes
    [InlineData("attributesInCreateRoadRequest", false, null, null)]
    [InlineData("attributesInCreateCyclePathRequest", false, null, null)]
    [InlineData("attributesInUpdateRoadRequest", false, null, null)]
    [InlineData("attributesInUpdateCyclePathRequest", false, null, null)]
    [InlineData("attributesInRoadResponse", false, "attributesInResponse", "lengthInMeters")]
    [InlineData("attributesInCyclePathResponse", false, "attributesInRoadResponse", "hasLaneForPedestrians")]
    // Road hierarchy: Relationships
    [InlineData("relationshipsInCreateRoadRequest", false, null, null)]
    [InlineData("relationshipsInCreateCyclePathRequest", false, null, null)]
    [InlineData("relationshipsInUpdateRoadRequest", false, null, null)]
    [InlineData("relationshipsInUpdateCyclePathRequest", false, null, null)]
    [InlineData("relationshipsInRoadResponse", false, "relationshipsInResponse", null)]
    [InlineData("relationshipsInCyclePathResponse", false, "relationshipsInRoadResponse", null)]
    // Road hierarchy: Resource Identifiers
    [InlineData("roadIdentifierInRequest", false, null, null)]
    [InlineData("cyclePathIdentifierInRequest", false, null, null)]
    [InlineData("roadIdentifierInResponse", false, null, "type|id|meta")]
    [InlineData("cyclePathIdentifierInResponse", false, "roadIdentifierInResponse", null)]
    // Road hierarchy: Atomic Operations
    [InlineData("createRoadOperation", false, null, null)]
    [InlineData("createCyclePathOperation", false, null, null)]
    [InlineData("updateRoadOperation", false, null, null)]
    [InlineData("updateCyclePathOperation", false, null, null)]
    [InlineData("deleteRoadOperation", false, null, null)]
    [InlineData("deleteCyclePathOperation", false, null, null)]
    public override async Task Component_schemas_have_expected_base_type(string schemaName, bool isAbstract, string? baseType, string? properties)
    {
        await base.Component_schemas_have_expected_base_type(schemaName, isAbstract, baseType, properties);
    }
}
