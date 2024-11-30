using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;

#pragma warning disable format

namespace OpenApiTests.ResourceInheritance.SubsetOfOperations;

public sealed class SubsetOfOperationsInheritanceTests : ResourceInheritanceTests
{
    public SubsetOfOperationsInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        : base(testContext, true)
    {
        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, SubsetOfOperationsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, SubsetOfOperationsOperationFilter>();
        });
    }

    [Theory]
    [InlineData(typeof(District), JsonApiEndpoints.None)]
    [InlineData(typeof(StaffMember), JsonApiEndpoints.None)]
    [InlineData(typeof(Building), JsonApiEndpoints.None)]
    [InlineData(typeof(Residence), JsonApiEndpoints.None)]
    [InlineData(typeof(FamilyHome), JsonApiEndpoints.None)]
    [InlineData(typeof(Mansion), JsonApiEndpoints.None)]
    [InlineData(typeof(Room), JsonApiEndpoints.None)]
    [InlineData(typeof(Kitchen), JsonApiEndpoints.None)]
    [InlineData(typeof(Bedroom), JsonApiEndpoints.None)]
    [InlineData(typeof(Bathroom), JsonApiEndpoints.None)]
    [InlineData(typeof(LivingRoom), JsonApiEndpoints.None)]
    [InlineData(typeof(Toilet), JsonApiEndpoints.None)]
    [InlineData(typeof(Road), JsonApiEndpoints.None)]
    [InlineData(typeof(CyclePath), JsonApiEndpoints.None)]
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
    [InlineData("dataInCreateRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings")]
    [InlineData("dataInUpdateRequest", false, "familyHomes|mansions|residences|buildings")]
    [InlineData("identifierInRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|staffMembers")]
    [InlineData("dataInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings")]
    [InlineData("buildingDataInResponse", true, "familyHomes|mansions|residences")]
    [InlineData("buildingIdentifierInResponse", false, "familyHomes|mansions|residences")]
    [InlineData("residenceDataInResponse", true, "familyHomes|mansions")]
    [InlineData("residenceIdentifierInResponse", true, "familyHomes|mansions")]
    [InlineData("roomDataInResponse", true, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomIdentifierInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roadDataInResponse", true, null)]
    [InlineData("roadIdentifierInResponse", false, null)]
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
    [InlineData("attributesInUpdateRoomRequest", null)]
    [InlineData("relationshipsInCreateRoomRequest", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("relationshipsInUpdateRoomRequest", null)]
    [InlineData("roomAttributesInResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomRelationshipsInResponse", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("attributesInCreateRoadRequest", null)]
    [InlineData("attributesInUpdateRoadRequest", null)]
    [InlineData("roadAttributesInResponse", null)]
    [InlineData("relationshipsInCreateRoadRequest", null)]
    [InlineData("relationshipsInUpdateRoadRequest", null)]
    [InlineData("roadRelationshipsInResponse", null)]
    [InlineData("atomicOperation",
        // @formatter:keep_existing_linebreaks true
        "" +
        "addResidence|updateResidence|" +
        "addFamilyHome|updateFamilyHome|addToFamilyHomeRooms|" +
        "addMansion|updateMansion|removeFromMansionRooms|removeFromMansionStaff|" +
        "addRoom|updateRoomResidence|" +
        "addBathroom|updateBathroomResidence|" +
        "addBedroom|updateBedroomResidence|" +
        "addKitchen|updateKitchenResidence|" +
        "addLivingRoom|updateLivingRoomResidence|" +
        "addToilet|updateToiletResidence" +
        "" +
        "" +
        "" +
        ""
        // @formatter:keep_existing_linebreaks restore
    )]
    public override async Task Expected_names_appear_in_openapi_discriminator_mapping(string schemaName, string? discriminatorValues)
    {
        await base.Expected_names_appear_in_openapi_discriminator_mapping(schemaName, discriminatorValues);
    }

    [Theory]
    [InlineData("buildingResourceType", "familyHomes|mansions|residences")]
    [InlineData("residenceResourceType", null)]
    [InlineData("familyHomeResourceType", "familyHomes")]
    [InlineData("mansionResourceType", "mansions")]
    [InlineData("roomResourceType", "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("bathroomResourceType", null)]
    [InlineData("bedroomResourceType", null)]
    [InlineData("kitchenResourceType", null)]
    [InlineData("livingRoomResourceType", null)]
    [InlineData("toiletResourceType", null)]
    [InlineData("roadResourceType", null)]
    [InlineData("cyclePathResourceType", null)]
    [InlineData("districtResourceType", null)]
    [InlineData("staffMemberResourceType", "staffMembers")]
    [InlineData("resourceType", "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|staffMembers")]
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
    [InlineData("deleteResidenceOperation", false, null, null)]
    [InlineData("deleteFamilyHomeOperation", false, null, null)]
    [InlineData("deleteMansionOperation", false, null, null)]
    [InlineData("updateResidenceRoomsRelationshipOperation", false, null, null)]
    [InlineData("updateFamilyHomeRoomsRelationshipOperation", false, null, null)]
    [InlineData("updateMansionRoomsRelationshipOperation", false, null, null)]
    [InlineData("updateMansionStaffRelationshipOperation", false, null, null)]
    [InlineData("addToResidenceRoomsRelationshipOperation", false, null, null)]
    [InlineData("addToFamilyHomeRoomsRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("addToMansionRoomsRelationshipOperation", false, null, null)]
    [InlineData("addToMansionStaffRelationshipOperation", false, null, null)]
    [InlineData("removeFromResidenceRoomsRelationshipOperation", false, null, null)]
    [InlineData("removeFromFamilyHomeRoomsRelationshipOperation", false, null, null)]
    [InlineData("removeFromMansionRoomsRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("removeFromMansionStaffRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    // Room hierarchy: Resource Data
    [InlineData("dataInCreateRoomRequest", true, "dataInCreateRequest", "lid|attributes|relationships")]
    [InlineData("dataInCreateBathroomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateBedroomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateKitchenRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateLivingRoomRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInCreateToiletRequest", false, "dataInCreateRoomRequest", null)]
    [InlineData("dataInUpdateRoomRequest", true, null, null)]
    [InlineData("dataInUpdateBathroomRequest", false, null, null)]
    [InlineData("dataInUpdateBedroomRequest", false, null, null)]
    [InlineData("dataInUpdateKitchenRequest", false, null, null)]
    [InlineData("dataInUpdateLivingRoomRequest", false, null, null)]
    [InlineData("dataInUpdateToiletRequest", false, null, null)]
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
    [InlineData("attributesInUpdateRoomRequest", true, null, null)]
    [InlineData("attributesInUpdateBathroomRequest", false, null, null)]
    [InlineData("attributesInUpdateBedroomRequest", false, null, null)]
    [InlineData("attributesInUpdateKitchenRequest", false, null, null)]
    [InlineData("attributesInUpdateLivingRoomRequest", false, null, null)]
    [InlineData("attributesInUpdateToiletRequest", false, null, null)]
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
    [InlineData("relationshipsInUpdateRoomRequest", true, null, null)]
    [InlineData("relationshipsInUpdateBathroomRequest", false, null, null)]
    [InlineData("relationshipsInUpdateBedroomRequest", false, null, null)]
    [InlineData("relationshipsInUpdateKitchenRequest", false, null, null)]
    [InlineData("relationshipsInUpdateLivingRoomRequest", false, null, null)]
    [InlineData("relationshipsInUpdateToiletRequest", false, null, null)]
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
    [InlineData("createRoomOperation", false, "atomicOperation", "op|data")]
    [InlineData("createBathroomOperation", false, "createRoomOperation", null)]
    [InlineData("createBedroomOperation", false, "createRoomOperation", null)]
    [InlineData("createKitchenOperation", false, "createRoomOperation", null)]
    [InlineData("createLivingRoomOperation", false, "createRoomOperation", null)]
    [InlineData("createToiletOperation", false, "createRoomOperation", null)]
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
    [InlineData("updateRoomResidenceRelationshipOperation", false, "atomicOperation", "op|ref|data")]
    [InlineData("updateBathroomResidenceRelationshipOperation", false, "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateBedroomResidenceRelationshipOperation", false, "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateKitchenResidenceRelationshipOperation", false, "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateLivingRoomResidenceRelationshipOperation", false, "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateToiletResidenceRelationshipOperation", false, "updateRoomResidenceRelationshipOperation", null)]
    // Road hierarchy: Resource Data
    [InlineData("dataInCreateRoadRequest", false, null, null)]
    [InlineData("dataInCreateCyclePathRequest", false, null, null)]
    [InlineData("dataInUpdateRoadRequest", false, null, null)]
    [InlineData("dataInUpdateCyclePathRequest", false, null, null)]
    [InlineData("roadDataInResponse", false, null, null)]
    [InlineData("cyclePathDataInResponse", false, null, null)]
    // Road hierarchy: Attributes
    [InlineData("attributesInCreateRoadRequest", false, null, null)]
    [InlineData("attributesInCreateCyclePathRequest", false, null, null)]
    [InlineData("attributesInUpdateRoadRequest", false, null, null)]
    [InlineData("attributesInUpdateCyclePathRequest", false, null, null)]
    [InlineData("roadAttributesInResponse", false, null, null)]
    [InlineData("cyclePathAttributesInResponse", false, null, null)]
    // Road hierarchy: Relationships
    [InlineData("relationshipsInCreateRoadRequest", false, null, null)]
    [InlineData("relationshipsInCreateCyclePathRequest", false, null, null)]
    [InlineData("relationshipsInUpdateRoadRequest", false, null, null)]
    [InlineData("relationshipsInUpdateCyclePathRequest", false, null, null)]
    [InlineData("roadRelationshipsInResponse", false, null, null)]
    [InlineData("cyclePathRelationshipsInResponse", false, null, null)]
    // Road hierarchy: Resource Identifiers
    [InlineData("roadIdentifierInRequest", false, null, null)]
    [InlineData("cyclePathIdentifierInRequest", false, null, null)]
    [InlineData("roadIdentifierInResponse", false, null, null)]
    [InlineData("cyclePathIdentifierInResponse", false, null, null)]
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
