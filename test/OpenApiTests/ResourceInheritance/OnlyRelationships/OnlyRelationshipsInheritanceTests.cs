using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;
using OpenApiTests.ResourceInheritance.Models;
using Xunit;

namespace OpenApiTests.ResourceInheritance.OnlyRelationships;

public sealed class OnlyRelationshipsInheritanceTests : ResourceInheritanceTests
{
    private const JsonApiEndpoints OnlyRelationshipEndpoints = JsonApiEndpoints.GetRelationship | JsonApiEndpoints.PostRelationship |
        JsonApiEndpoints.PatchRelationship | JsonApiEndpoints.DeleteRelationship;

    public OnlyRelationshipsInheritanceTests(OpenApiTestContext<OpenApiStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        : base(testContext)
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
    [InlineData("dataInCreateBuildingRequest", false, null)]
    [InlineData("dataInUpdateBuildingRequest", false, null)]
    [InlineData("buildingDataInResponse", true, null)]
    [InlineData("buildingIdentifierInRequest", false, "familyHomes|mansions|residences")]
    [InlineData("buildingIdentifierInResponse", false, "familyHomes|mansions|residences")]
    [InlineData("residenceDataInResponse", true, null)]
    [InlineData("residenceIdentifierInResponse", true, "familyHomes|mansions")]
    [InlineData("dataInCreateRoomRequest", false, null)]
    [InlineData("dataInUpdateRoomRequest", false, null)]
    [InlineData("roomDataInResponse", true, null)]
    [InlineData("roomIdentifierInRequest", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("roomIdentifierInResponse", false, "bathrooms|bedrooms|kitchens|livingRooms|toilets")]
    [InlineData("dataInResponse", false, "")]
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
        "addToDistrictBuildings|updateDistrictBuildings|removeFromDistrictBuildings"
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
    [InlineData("districtResourceType", "districts")]
    [InlineData("staffMemberResourceType", "staffMembers")]
    [InlineData("resourceType", "")] // Incorrect because omitted enum allows any string, but it's an extreme corner case.
    public override async Task Expected_names_appear_in_resource_type_enum(string schemaName, string? enumValues)
    {
        await base.Expected_names_appear_in_resource_type_enum(schemaName, enumValues);
    }

    [Theory]
    [InlineData("dataInResponse", null, "type|meta")]
    // Building hierarchy: Resource Data
    [InlineData("dataInCreateBuildingRequest", null, null)]
    [InlineData("dataInCreateResidenceRequest", null, null)]
    [InlineData("dataInCreateFamilyHomeRequest", null, null)]
    [InlineData("dataInCreateMansionRequest", null, null)]
    [InlineData("dataInUpdateBuildingRequest", null, null)]
    [InlineData("dataInUpdateResidenceRequest", null, null)]
    [InlineData("dataInUpdateFamilyHomeRequest", null, null)]
    [InlineData("dataInUpdateMansionRequest", null, null)]
    [InlineData("buildingDataInResponse", null, null)]
    [InlineData("residenceDataInResponse", null, null)]
    [InlineData("familyHomeDataInResponse", null, null)]
    [InlineData("mansionDataInResponse", null, null)]
    // Building hierarchy: Attributes
    [InlineData("attributesInCreateBuildingRequest", null, null)]
    [InlineData("attributesInCreateResidenceRequest", null, null)]
    [InlineData("attributesInCreateFamilyHomeRequest", null, null)]
    [InlineData("attributesInCreateMansionRequest", null, null)]
    // Building hierarchy: Relationships
    [InlineData("relationshipsInCreateBuildingRequest", null, null)]
    [InlineData("relationshipsInCreateResidenceRequest", null, null)]
    [InlineData("relationshipsInCreateFamilyHomeRequest", null, null)]
    [InlineData("relationshipsInCreateMansionRequest", null, null)]
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
    [InlineData("createResidenceOperation", null, null)]
    [InlineData("createFamilyHomeOperation", null, null)]
    [InlineData("createMansionOperation", null, null)]
    [InlineData("updateBuildingOperation", null, null)]
    [InlineData("updateResidenceOperation", null, null)]
    [InlineData("updateFamilyHomeOperation", null, null)]
    [InlineData("updateMansionOperation", null, null)]
    [InlineData("deleteBuildingOperation", null, null)]
    [InlineData("deleteResidenceOperation", null, null)]
    [InlineData("deleteFamilyHomeOperation", null, null)]
    [InlineData("deleteMansionOperation", null, null)]
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
    [InlineData("dataInCreateRoomRequest", null, null)]
    [InlineData("dataInCreateBathroomRequest", null, null)]
    [InlineData("dataInCreateBedroomRequest", null, null)]
    [InlineData("dataInCreateKitchenRequest", null, null)]
    [InlineData("dataInCreateLivingRoomRequest", null, null)]
    [InlineData("dataInCreateToiletRequest", null, null)]
    [InlineData("dataInUpdateRoomRequest", null, null)]
    [InlineData("dataInUpdateBathroomRequest", null, null)]
    [InlineData("dataInUpdateBedroomRequest", null, null)]
    [InlineData("dataInUpdateKitchenRequest", null, null)]
    [InlineData("dataInUpdateLivingRoomRequest", null, null)]
    [InlineData("dataInUpdateToiletRequest", null, null)]
    [InlineData("roomDataInResponse", null, null)]
    [InlineData("bathroomDataInResponse", null, null)]
    [InlineData("bedroomDataInResponse", null, null)]
    [InlineData("kitchenDataInResponse", null, null)]
    [InlineData("livingRoomDataInResponse", null, null)]
    [InlineData("toiletDataInResponse", null, null)]
    // Room hierarchy: Attributes
    [InlineData("attributesInCreateRoomRequest", null, null)]
    [InlineData("attributesInCreateBathroomRequest", null, null)]
    [InlineData("attributesInCreateBedroomRequest", null, null)]
    [InlineData("attributesInCreateKitchenRequest", null, null)]
    [InlineData("attributesInCreateLivingRoomRequest", null, null)]
    [InlineData("attributesInCreateToiletRequest", null, null)]
    [InlineData("attributesInUpdateBathroomRequest", null, null)]
    [InlineData("attributesInUpdateBedroomRequest", null, null)]
    [InlineData("attributesInUpdateKitchenRequest", null, null)]
    [InlineData("attributesInUpdateLivingRoomRequest", null, null)]
    [InlineData("attributesInUpdateToiletRequest", null, null)]
    [InlineData("roomAttributesInResponse", null, null)]
    [InlineData("bathroomAttributesInResponse", null, null)]
    [InlineData("bedroomAttributesInResponse", null, null)]
    [InlineData("kitchenAttributesInResponse", null, null)]
    [InlineData("livingRoomAttributesInResponse", null, null)]
    [InlineData("toiletAttributesInResponse", null, null)]
    // Room hierarchy: Relationships
    [InlineData("relationshipsInCreateRoomRequest", null, null)]
    [InlineData("relationshipsInCreateBathroomRequest", null, null)]
    [InlineData("relationshipsInCreateBedroomRequest", null, null)]
    [InlineData("relationshipsInCreateKitchenRequest", null, null)]
    [InlineData("relationshipsInCreateLivingRoomRequest", null, null)]
    [InlineData("relationshipsInCreateToiletRequest", null, null)]
    [InlineData("relationshipsInUpdateRoomRequest", null, null)]
    [InlineData("relationshipsInUpdateBathroomRequest", null, null)]
    [InlineData("relationshipsInUpdateBedroomRequest", null, null)]
    [InlineData("relationshipsInUpdateKitchenRequest", null, null)]
    [InlineData("relationshipsInUpdateLivingRoomRequest", null, null)]
    [InlineData("relationshipsInUpdateToiletRequest", null, null)]
    [InlineData("roomRelationshipsInResponse", null, null)]
    [InlineData("bathroomRelationshipsInResponse", null, null)]
    [InlineData("bedroomRelationshipsInResponse", null, null)]
    [InlineData("kitchenRelationshipsInResponse", null, null)]
    [InlineData("livingRoomRelationshipsInResponse", null, null)]
    [InlineData("toiletRelationshipsInResponse", null, null)]
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
    [InlineData("createBathroomOperation", null, null)]
    [InlineData("createBedroomOperation", null, null)]
    [InlineData("createKitchenOperation", null, null)]
    [InlineData("createLivingRoomOperation", null, null)]
    [InlineData("createToiletOperation", null, null)]
    [InlineData("updateRoomOperation", null, null)]
    [InlineData("updateBathroomOperation", null, null)]
    [InlineData("updateBedroomOperation", null, null)]
    [InlineData("updateKitchenOperation", null, null)]
    [InlineData("updateLivingRoomOperation", null, null)]
    [InlineData("updateToiletOperation", null, null)]
    [InlineData("deleteRoomOperation", null, null)]
    [InlineData("deleteBathroomOperation", null, null)]
    [InlineData("deleteBedroomOperation", null, null)]
    [InlineData("deleteKitchenOperation", null, null)]
    [InlineData("deleteLivingRoomOperation", null, null)]
    [InlineData("deleteToiletOperation", null, null)]
    [InlineData("updateRoomResidenceRelationshipOperation", "atomicOperation", "op|ref|data")]
    [InlineData("updateBathroomResidenceRelationshipOperation", "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateBedroomResidenceRelationshipOperation", "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateKitchenResidenceRelationshipOperation", "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateLivingRoomResidenceRelationshipOperation", "updateRoomResidenceRelationshipOperation", null)]
    [InlineData("updateToiletResidenceRelationshipOperation", "updateRoomResidenceRelationshipOperation", null)]
    public override async Task Component_schemas_have_expected_base_type(string schemaName, string? baseType, string? properties)
    {
        await base.Component_schemas_have_expected_base_type(schemaName, baseType, properties);
    }
}
