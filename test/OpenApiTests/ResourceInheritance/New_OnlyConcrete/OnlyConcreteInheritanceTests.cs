using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;

namespace OpenApiTests.ResourceInheritance.New_OnlyConcrete;

public sealed class OnlyConcreteInheritanceTests : ResourceInheritanceTests
{
    public OnlyConcreteInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        : base(testContext)
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
    [InlineData("dataInCreateBuildingRequest", false, "familyHomes|mansions|residences")]
    [InlineData("dataInUpdateBuildingRequest", false, "familyHomes|mansions|residences")]
    [InlineData("buildingDataInResponse", true, "familyHomes|mansions|residences")]
    [InlineData("buildingIdentifierInRequest", false, "familyHomes|mansions|residences")]
    [InlineData("buildingIdentifierInResponse", false, "familyHomes|mansions|residences")]
    [InlineData("residenceDataInResponse", true, "familyHomes|mansions")]
    [InlineData("residenceIdentifierInResponse", true, "familyHomes|mansions")]
    [InlineData("dataInCreateRoomRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("dataInUpdateRoomRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomDataInResponse", true, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomIdentifierInRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomIdentifierInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("dataInResponse", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|staffMembers")]
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
        "addDistrict|updateDistrict|removeDistrict|addToDistrictBuildings|updateDistrictBuildings|removeFromDistrictBuildings|" +
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
    [InlineData("districtResourceType", "districts")]
    [InlineData("staffMemberResourceType", "staffMembers")]
    [InlineData("resourceType", "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|districts|staffMembers")]
    public override async Task Expected_names_appear_in_resource_type_enum(string schemaName, string? enumValues)
    {
        await base.Expected_names_appear_in_resource_type_enum(schemaName, enumValues);
    }

    [Theory]
    [InlineData("dataInResponse", null, "type|meta")]
    // Building hierarchy: Resource Data
    [InlineData("dataInCreateBuildingRequest", null, "type|lid|attributes|relationships|meta")]
    [InlineData("dataInCreateResidenceRequest", "dataInCreateBuildingRequest", null)]
    [InlineData("dataInCreateFamilyHomeRequest", "dataInCreateResidenceRequest", null)]
    [InlineData("dataInCreateMansionRequest", "dataInCreateResidenceRequest", null)]
    [InlineData("dataInUpdateBuildingRequest", null, "type|id|lid|attributes|relationships|meta")]
    [InlineData("dataInUpdateResidenceRequest", "dataInUpdateBuildingRequest", null)]
    [InlineData("dataInUpdateFamilyHomeRequest", "dataInUpdateResidenceRequest", null)]
    [InlineData("dataInUpdateMansionRequest", "dataInUpdateResidenceRequest", null)]
    [InlineData("buildingDataInResponse", "dataInResponse", "id|attributes|relationships|links")]
    [InlineData("residenceDataInResponse", "buildingDataInResponse", null)]
    [InlineData("familyHomeDataInResponse", "residenceDataInResponse", null)]
    [InlineData("mansionDataInResponse", "residenceDataInResponse", null)]
    // Building hierarchy: Attributes
    [InlineData("attributesInCreateBuildingRequest", null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("attributesInCreateResidenceRequest", "attributesInCreateBuildingRequest", "numberOfResidents")]
    [InlineData("attributesInCreateFamilyHomeRequest", "attributesInCreateResidenceRequest", "floorCount")]
    [InlineData("attributesInCreateMansionRequest", "attributesInCreateResidenceRequest", "ownerName")]
    // Building hierarchy: Relationships
    [InlineData("relationshipsInCreateBuildingRequest", null, "openapi:discriminator")]
    [InlineData("relationshipsInCreateResidenceRequest", "relationshipsInCreateBuildingRequest", "rooms")]
    [InlineData("relationshipsInCreateFamilyHomeRequest", "relationshipsInCreateResidenceRequest", null)]
    [InlineData("relationshipsInCreateMansionRequest", "relationshipsInCreateResidenceRequest", "staff")]
    // Building hierarchy: Resource Identifiers
    [InlineData("buildingIdentifierInRequest", null, "type|id|lid|meta")]
    [InlineData("residenceIdentifierInRequest", "buildingIdentifierInRequest", null)]
    [InlineData("familyHomeIdentifierInRequest", "residenceIdentifierInRequest", null)]
    [InlineData("mansionIdentifierInRequest", "residenceIdentifierInRequest", null)]
    [InlineData("buildingIdentifierInResponse", null, "type|id|meta")]
    [InlineData("residenceIdentifierInResponse", "buildingIdentifierInResponse", null)]
    [InlineData("familyHomeIdentifierInResponse", "residenceIdentifierInResponse", null)]
    [InlineData("mansionIdentifierInResponse", "residenceIdentifierInResponse", null)]
    // Building hierarchy: Atomic Operations
    [InlineData("createBuildingOperation", null, null)]
    [InlineData("createResidenceOperation", "atomicOperation", "op|data")]
    [InlineData("createFamilyHomeOperation", "createResidenceOperation", null)]
    [InlineData("createMansionOperation", "createResidenceOperation", null)]
    [InlineData("updateBuildingOperation", null, null)]
    [InlineData("updateResidenceOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateFamilyHomeOperation", "updateResidenceOperation", null)]
    [InlineData("updateMansionOperation", "updateResidenceOperation", null)]
    [InlineData("deleteBuildingOperation", null, null)]
    [InlineData("deleteResidenceOperation", "atomicOperation", "op|ref")]
    [InlineData("deleteFamilyHomeOperation", "deleteResidenceOperation", null)]
    [InlineData("deleteMansionOperation", "deleteResidenceOperation", null)]
    [InlineData("updateResidenceRoomsRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateFamilyHomeRoomsRelationshipOperation", "updateResidenceRoomsRelationshipOperation", null)]
    [InlineData("updateMansionRoomsRelationshipOperation", "updateResidenceRoomsRelationshipOperation", null)]
    [InlineData("updateMansionStaffRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("addToResidenceRoomsRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("addToFamilyHomeRoomsRelationshipOperation", "addToResidenceRoomsRelationshipOperation", null)]
    [InlineData("addToMansionRoomsRelationshipOperation", "addToResidenceRoomsRelationshipOperation", null)]
    [InlineData("addToMansionStaffRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("removeFromResidenceRoomsRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("removeFromFamilyHomeRoomsRelationshipOperation", "removeFromResidenceRoomsRelationshipOperation", null)]
    [InlineData("removeFromMansionRoomsRelationshipOperation", "removeFromResidenceRoomsRelationshipOperation", null)]
    [InlineData("removeFromMansionStaffRelationshipOperation", "atomicOperation", "op|ref|data")]
    // Room hierarchy: Resource Data
    [InlineData("dataInCreateRoomRequest", null, "type|lid|attributes|relationships|meta")]
    [InlineData("dataInCreateBathroomRequest", "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateBedroomRequest", "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateKitchenRequest", "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateLivingRoomRequest", "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateToiletRequest", "dataInCreateRoomRequest", null)]
    [InlineData("dataInUpdateRoomRequest", null, "type|id|lid|attributes|relationships|meta")]
    [InlineData("dataInUpdateBathroomRequest", "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateBedroomRequest", "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateKitchenRequest", "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateLivingRoomRequest", "dataInUpdateRoomRequest", null)]
    [InlineData("dataInUpdateToiletRequest", "dataInUpdateRoomRequest", null)]
    [InlineData("roomDataInResponse", "dataInResponse", "id|attributes|relationships|links")]
    [InlineData("bathroomDataInResponse", "roomDataInResponse", null)]
    [InlineData("bedroomDataInResponse", "roomDataInResponse", null)]
    [InlineData("kitchenDataInResponse", "roomDataInResponse", null)]
    [InlineData("livingRoomDataInResponse", "roomDataInResponse", null)]
    [InlineData("toiletDataInResponse", "roomDataInResponse", null)]
    // Room hierarchy: Attributes
    [InlineData("attributesInCreateRoomRequest", null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("attributesInCreateBathroomRequest", "attributesInCreateRoomRequest", "hasBath")]
    [InlineData("attributesInCreateBedroomRequest", "attributesInCreateRoomRequest", "bedCount")]
    [InlineData("attributesInCreateKitchenRequest", "attributesInCreateRoomRequest", "hasPantry")]
    [InlineData("attributesInCreateLivingRoomRequest", "attributesInCreateRoomRequest", "hasDiningTable")]
    [InlineData("attributesInCreateToiletRequest", "attributesInCreateRoomRequest", "hasSink")]
    [InlineData("attributesInUpdateBathroomRequest", "attributesInUpdateRoomRequest", "hasBath")]
    [InlineData("attributesInUpdateBedroomRequest", "attributesInUpdateRoomRequest", "bedCount")]
    [InlineData("attributesInUpdateKitchenRequest", "attributesInUpdateRoomRequest", "hasPantry")]
    [InlineData("attributesInUpdateLivingRoomRequest", "attributesInUpdateRoomRequest", "hasDiningTable")]
    [InlineData("attributesInUpdateToiletRequest", "attributesInUpdateRoomRequest", "hasSink")]
    [InlineData("roomAttributesInResponse", null, "surfaceInSquareMeters|openapi:discriminator")]
    [InlineData("bathroomAttributesInResponse", "roomAttributesInResponse", "hasBath")]
    [InlineData("bedroomAttributesInResponse", "roomAttributesInResponse", "bedCount")]
    [InlineData("kitchenAttributesInResponse", "roomAttributesInResponse", "hasPantry")]
    [InlineData("livingRoomAttributesInResponse", "roomAttributesInResponse", "hasDiningTable")]
    [InlineData("toiletAttributesInResponse", "roomAttributesInResponse", "hasSink")]
    // Room hierarchy: Relationships
    [InlineData("relationshipsInCreateRoomRequest", null, "residence|openapi:discriminator")]
    [InlineData("relationshipsInCreateBathroomRequest", "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateBedroomRequest", "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateKitchenRequest", "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateLivingRoomRequest", "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInCreateToiletRequest", "relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInUpdateRoomRequest", null, "residence|openapi:discriminator")]
    [InlineData("relationshipsInUpdateBathroomRequest", "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateBedroomRequest", "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateKitchenRequest", "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateLivingRoomRequest", "relationshipsInUpdateRoomRequest", null)]
    [InlineData("relationshipsInUpdateToiletRequest", "relationshipsInUpdateRoomRequest", null)]
    [InlineData("roomRelationshipsInResponse", null, "residence|openapi:discriminator")]
    [InlineData("bathroomRelationshipsInResponse", "roomRelationshipsInResponse", null)]
    [InlineData("bedroomRelationshipsInResponse", "roomRelationshipsInResponse", null)]
    [InlineData("kitchenRelationshipsInResponse", "roomRelationshipsInResponse", null)]
    [InlineData("livingRoomRelationshipsInResponse", "roomRelationshipsInResponse", null)]
    [InlineData("toiletRelationshipsInResponse", "roomRelationshipsInResponse", null)]
    // Room hierarchy: Resource Identifiers
    [InlineData("roomIdentifierInRequest", null, "type|id|lid|meta")]
    [InlineData("bathroomIdentifierInRequest", "roomIdentifierInRequest", null)]
    [InlineData("bedroomIdentifierInRequest", "roomIdentifierInRequest", null)]
    [InlineData("kitchenIdentifierInRequest", "roomIdentifierInRequest", null)]
    [InlineData("livingRoomIdentifierInRequest", "roomIdentifierInRequest", null)]
    [InlineData("toiletIdentifierInRequest", "roomIdentifierInRequest", null)]
    [InlineData("roomIdentifierInResponse", null, "type|id|meta")]
    [InlineData("bathroomIdentifierInResponse", "roomIdentifierInResponse", null)]
    [InlineData("bedroomIdentifierInResponse", "roomIdentifierInResponse", null)]
    [InlineData("kitchenIdentifierInResponse", "roomIdentifierInResponse", null)]
    [InlineData("livingRoomIdentifierInResponse", "roomIdentifierInResponse", null)]
    [InlineData("toiletIdentifierInResponse", "roomIdentifierInResponse", null)]
    // Room hierarchy: Atomic Operations
    [InlineData("createRoomOperation", null, null)]
    [InlineData("createBathroomOperation", "atomicOperation", "op|data")]
    [InlineData("createBedroomOperation", "atomicOperation", "op|data")]
    [InlineData("createKitchenOperation", "atomicOperation", "op|data")]
    [InlineData("createLivingRoomOperation", "atomicOperation", "op|data")]
    [InlineData("createToiletOperation", "atomicOperation", "op|data")]
    [InlineData("updateRoomOperation", null, null)]
    [InlineData("updateBathroomOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateBedroomOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateKitchenOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateLivingRoomOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateToiletOperation", "atomicOperation", "op|ref|data")]
    [InlineData("deleteRoomOperation", null, null)]
    [InlineData("deleteBathroomOperation", "atomicOperation", "op|ref")]
    [InlineData("deleteBedroomOperation", "atomicOperation", "op|ref")]
    [InlineData("deleteKitchenOperation", "atomicOperation", "op|ref")]
    [InlineData("deleteLivingRoomOperation", "atomicOperation", "op|ref")]
    [InlineData("deleteToiletOperation", "atomicOperation", "op|ref")]
    [InlineData("updateRoomResidenceRelationshipOperation", null, null)]
    [InlineData("updateBathroomResidenceRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateBedroomResidenceRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateKitchenResidenceRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateLivingRoomResidenceRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateToiletResidenceRelationshipOperation", "atomicOperation", "op|ref|data")]
    public override async Task Component_schemas_have_expected_base_type(string schemaName, string? baseType, string? properties)
    {
        await base.Component_schemas_have_expected_base_type(schemaName, baseType, properties);
    }
}
