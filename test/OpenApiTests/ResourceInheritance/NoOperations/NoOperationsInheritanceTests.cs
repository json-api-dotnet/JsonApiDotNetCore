using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;

#pragma warning disable format

namespace OpenApiTests.ResourceInheritance.NoOperations;

public sealed class NoOperationsInheritanceTests : ResourceInheritanceTests
{
    public NoOperationsInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        : base(testContext, true)
    {
        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, NoOperationsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, NoOperationsOperationFilter>();
        });
    }

    [Theory]
    [InlineData(typeof(District), JsonApiEndpoints.All)]
    [InlineData(typeof(StaffMember), JsonApiEndpoints.All)]
    [InlineData(typeof(Building), JsonApiEndpoints.All)]
    [InlineData(typeof(Residence), JsonApiEndpoints.All)]
    [InlineData(typeof(FamilyHome), JsonApiEndpoints.All)]
    [InlineData(typeof(Mansion), JsonApiEndpoints.All)]
    [InlineData(typeof(Room), JsonApiEndpoints.All)]
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
    [InlineData("dataInCreateRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("dataInUpdateRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("identifierInRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|roads|cyclePaths|staffMembers")]
    [InlineData("dataInResponse", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("buildingDataInResponse", true, "familyHomes|mansions|residences")]
    [InlineData("buildingIdentifierInResponse", false, "familyHomes|mansions|residences")]
    [InlineData("residenceDataInResponse", true, "familyHomes|mansions")]
    [InlineData("residenceIdentifierInResponse", true, "familyHomes|mansions")]
    [InlineData("roomDataInResponse", true, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomIdentifierInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roadDataInResponse", true, "cyclePaths")]
    [InlineData("roadIdentifierInResponse", false, "cyclePaths")]
    public override async Task Expected_names_appear_in_type_discriminator_mapping(string schemaName, bool isWrapped, string? discriminatorValues)
    {
        await base.Expected_names_appear_in_type_discriminator_mapping(schemaName, isWrapped, discriminatorValues);
    }

    [Theory]
    [InlineData("attributesInCreateBuildingRequest", "familyHomes|mansions|residences")]
    [InlineData("attributesInUpdateBuildingRequest", "familyHomes|mansions|residences")]
    [InlineData("buildingAttributesInResponse", "familyHomes|mansions|residences")]
    [InlineData("relationshipsInCreateBuildingRequest", "familyHomes|mansions|residences")]
    [InlineData("relationshipsInUpdateBuildingRequest", "familyHomes|mansions|residences")]
    [InlineData("buildingRelationshipsInResponse", "familyHomes|mansions|residences")]
    [InlineData("attributesInCreateRoomRequest", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("attributesInUpdateRoomRequest", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("relationshipsInCreateRoomRequest", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("relationshipsInUpdateRoomRequest", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomAttributesInResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomRelationshipsInResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("attributesInCreateRoadRequest", "cyclePaths")]
    [InlineData("attributesInUpdateRoadRequest", "cyclePaths")]
    [InlineData("roadAttributesInResponse", "cyclePaths")]
    [InlineData("relationshipsInCreateRoadRequest", "cyclePaths")]
    [InlineData("relationshipsInUpdateRoadRequest", "cyclePaths")]
    [InlineData("roadRelationshipsInResponse", "cyclePaths")]
    [InlineData("atomicOperation", "")]
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
    [InlineData("dataInCreateRequest", true, null, "type|meta")]
    [InlineData("dataInUpdateRequest", true, null, "type|meta")]
    [InlineData("identifierInRequest", true, null, "type|meta")]
    [InlineData("dataInResponse", true, null, "type|meta")]
    [InlineData("atomicOperation", true, null, "openapi:discriminator|meta")]
    // Building hierarchy: Resource Data
    [InlineData("dataInCreateBuildingRequest", true, "dataInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateResidenceRequest", false, "dataInCreateBuildingRequest", null)]
    [InlineData("dataInCreateFamilyHomeRequest", false, "dataInCreateResidenceRequest", null)]
    [InlineData("dataInCreateMansionRequest", false, "dataInCreateResidenceRequest", null)]
    [InlineData("dataInUpdateBuildingRequest", true, "dataInUpdateRequest", "id|lid|attributes|relationships")]
    [InlineData("dataInUpdateResidenceRequest", false, "dataInUpdateBuildingRequest", null)]
    [InlineData("dataInUpdateFamilyHomeRequest", false, "dataInUpdateResidenceRequest", null)]
    [InlineData("dataInUpdateMansionRequest", false, "dataInUpdateResidenceRequest", null)]
    [InlineData("buildingDataInResponse", true, "dataInResponse", "id|attributes|relationships|links")]
    [InlineData("residenceDataInResponse", false, "buildingDataInResponse", null)]
    [InlineData("familyHomeDataInResponse", false, "residenceDataInResponse", null)]
    [InlineData("mansionDataInResponse", false, "residenceDataInResponse", null)]
    // Building hierarchy: Attributes
    [InlineData("attributesInCreateBuildingRequest", true, null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("attributesInCreateResidenceRequest", false, "attributesInCreateBuildingRequest", "numberOfResidents")]
    [InlineData("attributesInCreateFamilyHomeRequest", false, "attributesInCreateResidenceRequest", "floorCount")]
    [InlineData("attributesInCreateMansionRequest", false, "attributesInCreateResidenceRequest", "ownerName")]
    [InlineData("attributesInUpdateBuildingRequest", true, null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("attributesInUpdateResidenceRequest", false, "attributesInUpdateBuildingRequest", "numberOfResidents")]
    [InlineData("attributesInUpdateFamilyHomeRequest", false, "attributesInUpdateResidenceRequest", "floorCount")]
    [InlineData("attributesInUpdateMansionRequest", false, "attributesInUpdateResidenceRequest", "ownerName")]
    [InlineData("buildingAttributesInResponse", true, null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("residenceAttributesInResponse", false, "buildingAttributesInResponse", "numberOfResidents")]
    [InlineData("familyHomeAttributesInResponse", false, "residenceAttributesInResponse", "floorCount")]
    [InlineData("mansionAttributesInResponse", false, "residenceAttributesInResponse", "ownerName")]
    // Building hierarchy: Relationships
    [InlineData("relationshipsInCreateBuildingRequest", true, null, "openapi:discriminator")]
    [InlineData("relationshipsInCreateResidenceRequest", false, "relationshipsInCreateBuildingRequest", "rooms")]
    [InlineData("relationshipsInCreateFamilyHomeRequest", false, "relationshipsInCreateResidenceRequest", null)]
    [InlineData("relationshipsInCreateMansionRequest", false, "relationshipsInCreateResidenceRequest", "staff")]
    [InlineData("relationshipsInUpdateBuildingRequest", true, null, "openapi:discriminator")]
    [InlineData("relationshipsInUpdateResidenceRequest", false, "relationshipsInUpdateBuildingRequest", "rooms")]
    [InlineData("relationshipsInUpdateFamilyHomeRequest", false, "relationshipsInUpdateResidenceRequest", null)]
    [InlineData("relationshipsInUpdateMansionRequest", false, "relationshipsInUpdateResidenceRequest", "staff")]
    [InlineData("buildingRelationshipsInResponse", true, null, "openapi:discriminator")]
    [InlineData("residenceRelationshipsInResponse", false, "buildingRelationshipsInResponse", "rooms")]
    [InlineData("familyHomeRelationshipsInResponse", false, "residenceRelationshipsInResponse", null)]
    [InlineData("mansionRelationshipsInResponse", false, "residenceRelationshipsInResponse", "staff")]
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
    [InlineData("dataInCreateRoomRequest", true, "dataInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateBathroomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateBedroomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateKitchenRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateLivingRoomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateToiletRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInUpdateRoomRequest", true, "dataInUpdateRequest", "id|lid|attributes|relationships")]
    [InlineData("dataInUpdateBathroomRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateBedroomRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateKitchenRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateLivingRoomRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateToiletRequest", false, "dataInUpdateRoomRequest", null)]
    [InlineData("roomDataInResponse", true, "dataInResponse", "id|attributes|relationships|links")]
    [InlineData("bathroomDataInResponse", false, "roomDataInResponse", null)]
    [InlineData("bedroomDataInResponse", false, "roomDataInResponse", null)]
    [InlineData("kitchenDataInResponse", false, "roomDataInResponse", null)]
    [InlineData("livingRoomDataInResponse", false, "roomDataInResponse", null)]
    [InlineData("toiletDataInResponse", false, "roomDataInResponse", null)]
    // Room hierarchy: Attributes
    [InlineData("attributesInCreateRoomRequest", true, null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("attributesInCreateBathroomRequest", false, "attributesInCreateRoomRequest", "hasBath")]
    [InlineData("attributesInCreateBedroomRequest", false, "attributesInCreateRoomRequest", "bedCount")]
    [InlineData("attributesInCreateKitchenRequest", false, "attributesInCreateRoomRequest", "hasPantry")]
    [InlineData("attributesInCreateLivingRoomRequest", false, "attributesInCreateRoomRequest", "hasDiningTable")]
    [InlineData("attributesInCreateToiletRequest", false, "attributesInCreateRoomRequest", "hasSink")]
    [InlineData("attributesInUpdateRoomRequest", true, null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("attributesInUpdateBathroomRequest", false, "attributesInUpdateRoomRequest", "hasBath")]
    [InlineData("attributesInUpdateBedroomRequest", false, "attributesInUpdateRoomRequest", "bedCount")]
    [InlineData("attributesInUpdateKitchenRequest", false, "attributesInUpdateRoomRequest", "hasPantry")]
    [InlineData("attributesInUpdateLivingRoomRequest", false, "attributesInUpdateRoomRequest", "hasDiningTable")]
    [InlineData("attributesInUpdateToiletRequest", false, "attributesInUpdateRoomRequest", "hasSink")]
    [InlineData("roomAttributesInResponse", true, null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("bathroomAttributesInResponse", false, "roomAttributesInResponse", "hasBath")]
    [InlineData("bedroomAttributesInResponse", false, "roomAttributesInResponse", "bedCount")]
    [InlineData("kitchenAttributesInResponse", false, "roomAttributesInResponse", "hasPantry")]
    [InlineData("livingRoomAttributesInResponse", false, "roomAttributesInResponse", "hasDiningTable")]
    [InlineData("toiletAttributesInResponse", false, "roomAttributesInResponse", "hasSink")]
    // Room hierarchy: Relationships
    [InlineData("relationshipsInCreateRoomRequest", true, null, "residence|openapi:discriminator")]
    [InlineData("relationshipsInCreateBathroomRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateBedroomRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateKitchenRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateLivingRoomRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateToiletRequest", false, "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInUpdateRoomRequest", true, null, "residence|openapi:discriminator")]
    [InlineData("relationshipsInUpdateBathroomRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateBedroomRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateKitchenRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateLivingRoomRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateToiletRequest", false, "relationshipsInUpdateRoomRequest", null)]
    [InlineData("roomRelationshipsInResponse", true, null, "residence|openapi:discriminator")]
    [InlineData("bathroomRelationshipsInResponse", false, "roomRelationshipsInResponse", null)]
    [InlineData("bedroomRelationshipsInResponse", false, "roomRelationshipsInResponse", null)]
    [InlineData("kitchenRelationshipsInResponse", false, "roomRelationshipsInResponse", null)]
    [InlineData("livingRoomRelationshipsInResponse", false, "roomRelationshipsInResponse", null)]
    [InlineData("toiletRelationshipsInResponse", false, "roomRelationshipsInResponse", null)]
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
    [InlineData("dataInCreateRoadRequest", false, "dataInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateCyclePathRequest", false, "dataInCreateRoadRequest", null)]
    [InlineData("dataInUpdateRoadRequest", false, "dataInUpdateRequest", "id|lid|attributes|relationships")]
    [InlineData("dataInUpdateCyclePathRequest", false, "dataInUpdateRoadRequest", null)]
    [InlineData("roadDataInResponse", false, "dataInResponse", "id|attributes|relationships|links")]
    [InlineData("cyclePathDataInResponse", false, "roadDataInResponse", null)]
    // Road hierarchy: Attributes
    [InlineData("attributesInCreateRoadRequest", false, null, "lengthInMeters|openapi:discriminator")]
    [InlineData("attributesInCreateCyclePathRequest", false, "attributesInCreateRoadRequest", "hasLaneForPedestrians")]
    [InlineData("attributesInUpdateRoadRequest", false, null, "lengthInMeters|openapi:discriminator")]
    [InlineData("attributesInUpdateCyclePathRequest", false, "attributesInUpdateRoadRequest", "hasLaneForPedestrians")]
    [InlineData("roadAttributesInResponse", false, null, "lengthInMeters|openapi:discriminator")]
    [InlineData("cyclePathAttributesInResponse", false, "roadAttributesInResponse", "hasLaneForPedestrians")]
    // Road hierarchy: Relationships
    [InlineData("relationshipsInCreateRoadRequest", false, null, "openapi:discriminator")]
    [InlineData("relationshipsInCreateCyclePathRequest", false, "relationshipsInCreateRoadRequest", null)]
    [InlineData("relationshipsInUpdateRoadRequest", false, null, "openapi:discriminator")]
    [InlineData("relationshipsInUpdateCyclePathRequest", false, "relationshipsInUpdateRoadRequest", null)]
    [InlineData("roadRelationshipsInResponse", false, null, "openapi:discriminator")]
    [InlineData("cyclePathRelationshipsInResponse", false, "roadRelationshipsInResponse", null)]
    // Road hierarchy: Resource Identifiers
    [InlineData("roadIdentifierInRequest", false, "identifierInRequest", "id|lid")]
    [InlineData("cyclePathIdentifierInRequest", false, "roadIdentifierInRequest", null)]
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
