using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;

#pragma warning disable format

namespace OpenApiTests.ResourceInheritance.OnlyRelationships;

public sealed class OnlyRelationshipsInheritanceTests : ResourceInheritanceTests
{
    private const JsonApiEndpoints OnlyRelationshipEndpoints = JsonApiEndpoints.GetRelationship | JsonApiEndpoints.PostRelationship |
        JsonApiEndpoints.PatchRelationship | JsonApiEndpoints.DeleteRelationship;

    public OnlyRelationshipsInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        : base(testContext, true)
    {
        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<IJsonApiEndpointFilter, OnlyRelationshipsEndpointFilter>();
            services.AddSingleton<IAtomicOperationFilter, OnlyRelationshipsOperationFilter>();
        });
    }

    [Theory]
    [InlineData(typeof(District), OnlyRelationshipEndpoints)]
    [InlineData(typeof(StaffMember), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Building), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Residence), OnlyRelationshipEndpoints)]
    [InlineData(typeof(FamilyHome), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Mansion), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Room), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Kitchen), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Bedroom), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Bathroom), OnlyRelationshipEndpoints)]
    [InlineData(typeof(LivingRoom), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Toilet), OnlyRelationshipEndpoints)]
    [InlineData(typeof(Road), OnlyRelationshipEndpoints)]
    [InlineData(typeof(CyclePath), OnlyRelationshipEndpoints)]
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
    [InlineData("dataInCreateRequest", false, null)]
    [InlineData("dataInUpdateRequest", false, null)]
    [InlineData("identifierInRequest", false,
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|roads|cyclePaths|staffMembers")]
    [InlineData("dataInResponse", false, "")]
    [InlineData("buildingDataInResponse", true, null)]
    [InlineData("buildingIdentifierInResponse", false, "familyHomes|mansions|residences")]
    [InlineData("residenceDataInResponse", true, null)]
    [InlineData("residenceIdentifierInResponse", true, "familyHomes|mansions")]
    [InlineData("roomDataInResponse", true, null)]
    [InlineData("roomIdentifierInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roadDataInResponse", true, null)]
    [InlineData("roadIdentifierInResponse", false, "cyclePaths")]
    public override async Task Expected_names_appear_in_type_discriminator_mapping(string schemaName, bool isWrapped, string? discriminatorValues)
    {
        await base.Expected_names_appear_in_type_discriminator_mapping(schemaName, isWrapped, discriminatorValues);
    }

    [Theory]
    [InlineData("attributesInCreateBuildingRequest", null)]
    [InlineData("attributesInUpdateBuildingRequest", null)]
    [InlineData("buildingAttributesInResponse", null)]
    [InlineData("relationshipsInCreateBuildingRequest", null)]
    [InlineData("relationshipsInUpdateBuildingRequest", null)]
    [InlineData("buildingRelationshipsInResponse", null)]
    [InlineData("attributesInCreateRoomRequest", null)]
    [InlineData("attributesInUpdateRoomRequest", null)]
    [InlineData("relationshipsInCreateRoomRequest", null)]
    [InlineData("relationshipsInUpdateRoomRequest", null)]
    [InlineData("roomAttributesInResponse", null)]
    [InlineData("roomRelationshipsInResponse", null)]
    [InlineData("attributesInCreateRoadRequest", null)]
    [InlineData("attributesInUpdateRoadRequest", null)]
    [InlineData("roadAttributesInResponse", null)]
    [InlineData("relationshipsInCreateRoadRequest", null)]
    [InlineData("relationshipsInUpdateRoadRequest", null)]
    [InlineData("roadRelationshipsInResponse", null)]
    [InlineData("atomicOperation",
        // @formatter:keep_existing_linebreaks true
        "addToResidenceRooms|updateResidenceRooms|removeFromResidenceRooms|" +
        "addToFamilyHomeRooms|updateFamilyHomeRooms|removeFromFamilyHomeRooms|" +
        "addToMansionRooms|updateMansionRooms|removeFromMansionRooms|addToMansionStaff|updateMansionStaff|removeFromMansionStaff|" +
        "updateRoomResidence|" +
        "updateBathroomResidence|" +
        "updateBedroomResidence|" +
        "updateKitchenResidence|" +
        "updateLivingRoomResidence|" +
        "updateToiletResidence|" +
        "addToDistrictBuildings|updateDistrictBuildings|removeFromDistrictBuildings|addToDistrictRoads|updateDistrictRoads|removeFromDistrictRoads"
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
    [InlineData("bathroomResourceType", null)]
    [InlineData("bedroomResourceType", null)]
    [InlineData("kitchenResourceType", null)]
    [InlineData("livingRoomResourceType", null)]
    [InlineData("toiletResourceType", null)]
    [InlineData("roadResourceType", "roads|cyclePaths")]
    [InlineData("cyclePathResourceType", null)]
    [InlineData("districtResourceType", "districts")]
    [InlineData("staffMemberResourceType", "staffMembers")]
    [InlineData("resourceType",
        "bathrooms|bedrooms|kitchens|livingRooms|toilets|rooms|familyHomes|mansions|residences|buildings|roads|cyclePaths|staffMembers")]
    public override async Task Expected_names_appear_in_resource_type_enum(string schemaName, string? enumValues)
    {
        await base.Expected_names_appear_in_resource_type_enum(schemaName, enumValues);
    }

    [Theory]
    [InlineData("dataInCreateRequest", true, null, null)]
    [InlineData("dataInUpdateRequest", true, null, null)]
    [InlineData("identifierInRequest", true, null, "type|meta")]
    [InlineData("dataInResponse", true, null, "type|meta")]
    [InlineData("atomicOperation", true, null, "openapi:discriminator|meta")]
    // Building hierarchy: Resource Data
    [InlineData("dataInCreateBuildingRequest", true, null, null)]
    [InlineData("dataInCreateResidenceRequest", false, null, null)]
    [InlineData("dataInCreateFamilyHomeRequest", false, null, null)]
    [InlineData("dataInCreateMansionRequest", false, null, null)]
    [InlineData("dataInUpdateBuildingRequest", true, null, null)]
    [InlineData("dataInUpdateResidenceRequest", false, null, null)]
    [InlineData("dataInUpdateFamilyHomeRequest", false, null, null)]
    [InlineData("dataInUpdateMansionRequest", false, null, null)]
    [InlineData("buildingDataInResponse", true, null, null)]
    [InlineData("residenceDataInResponse", false, null, null)]
    [InlineData("familyHomeDataInResponse", false, null, null)]
    [InlineData("mansionDataInResponse", false, null, null)]
    // Building hierarchy: Attributes
    [InlineData("attributesInCreateBuildingRequest", true, null, null)]
    [InlineData("attributesInCreateResidenceRequest", false, null, null)]
    [InlineData("attributesInCreateFamilyHomeRequest", false, null, null)]
    [InlineData("attributesInCreateMansionRequest", false, null, null)]
    [InlineData("attributesInUpdateBuildingRequest", true, null, null)]
    [InlineData("attributesInUpdateResidenceRequest", false, null, null)]
    [InlineData("attributesInUpdateFamilyHomeRequest", false, null, null)]
    [InlineData("attributesInUpdateMansionRequest", false, null, null)]
    [InlineData("buildingAttributesInResponse", true, null, null)]
    [InlineData("residenceAttributesInResponse", false, null, null)]
    [InlineData("familyHomeAttributesInResponse", false, null, null)]
    [InlineData("mansionAttributesInResponse", false, null, null)]
    // Building hierarchy: Relationships
    [InlineData("relationshipsInCreateBuildingRequest", true, null, null)]
    [InlineData("relationshipsInCreateResidenceRequest", false, null, null)]
    [InlineData("relationshipsInCreateFamilyHomeRequest", false, null, null)]
    [InlineData("relationshipsInCreateMansionRequest", false, null, null)]
    [InlineData("relationshipsInUpdateBuildingRequest", true, null, null)]
    [InlineData("relationshipsInUpdateResidenceRequest", false, null, null)]
    [InlineData("relationshipsInUpdateFamilyHomeRequest", false, null, null)]
    [InlineData("relationshipsInUpdateMansionRequest", false, null, null)]
    [InlineData("buildingRelationshipsInResponse", true, null, null)]
    [InlineData("residenceRelationshipsInResponse", false, null, null)]
    [InlineData("familyHomeRelationshipsInResponse", false, null, null)]
    [InlineData("mansionRelationshipsInResponse", false, null, null)]
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
    [InlineData("roomDataInResponse", true, null, null)]
    [InlineData("bathroomDataInResponse", false, null, null)]
    [InlineData("bedroomDataInResponse", false, null, null)]
    [InlineData("kitchenDataInResponse", false, null, null)]
    [InlineData("livingRoomDataInResponse", false, null, null)]
    [InlineData("toiletDataInResponse", false, null, null)]
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
    [InlineData("roomAttributesInResponse", true, null, null)]
    [InlineData("bathroomAttributesInResponse", false, null, null)]
    [InlineData("bedroomAttributesInResponse", false, null, null)]
    [InlineData("kitchenAttributesInResponse", false, null, null)]
    [InlineData("livingRoomAttributesInResponse", false, null, null)]
    [InlineData("toiletAttributesInResponse", false, null, null)]
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
    [InlineData("roomRelationshipsInResponse", true, null, null)]
    [InlineData("bathroomRelationshipsInResponse", false, null, null)]
    [InlineData("bedroomRelationshipsInResponse", false, null, null)]
    [InlineData("kitchenRelationshipsInResponse", false, null, null)]
    [InlineData("livingRoomRelationshipsInResponse", false, null, null)]
    [InlineData("toiletRelationshipsInResponse", false, null, null)]
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
