using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;

#pragma warning disable format

namespace OpenApiTests.ResourceInheritance.New_OnlyConcrete;

public sealed class OnlyConcreteInheritanceTests : ResourceInheritanceTests
{
    public OnlyConcreteInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        : base(testContext, true)
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
    [InlineData("dataInCreateRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("dataInUpdateRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
    [InlineData("identifierInRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|roads|cyclePaths|staffMembers")]
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
    [InlineData("atomicOperation",
        // @formatter:keep_existing_linebreaks true
        "addResidence|updateResidence|removeResidence|addToResidenceRooms|updateResidenceRooms|removeFromResidenceRooms|" +
        "addFamilyHome|updateFamilyHome|removeFamilyHome|addToFamilyHomeRooms|updateFamilyHomeRooms|removeFromFamilyHomeRooms|" +
        "addMansion|updateMansion|removeMansion|addToMansionRooms|updateMansionRooms|removeFromMansionRooms|addToMansionStaff|updateMansionStaff|removeFromMansionStaff|" +
        "addBathroom|updateBathroom|removeBathroom|updateBathroomResidence|" +
        "addBedroom|updateBedroom|removeBedroom|updateBedroomResidence|" +
        "addKitchen|updateKitchen|removeKitchen|updateKitchenResidence|" +
        "addLivingRoom|updateLivingRoom|removeLivingRoom|updateLivingRoomResidence|" +
        "addToilet|updateToilet|removeToilet|updateToiletResidence|" +
        "addDistrict|updateDistrict|removeDistrict|addToDistrictBuildings|updateDistrictBuildings|removeFromDistrictBuildings|addToDistrictRoads|updateDistrictRoads|removeFromDistrictRoads|" +
        "addRoad|updateRoad|removeRoad|" +
        "addCyclePath|updateCyclePath|removeCyclePath|" +
        "addStaffMember|updateStaffMember|removeStaffMember"
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
