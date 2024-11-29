using OpenApiNSwagClientTests.NamingConventions.CamelCase.GeneratedCode;
using Xunit;
using GeneratedClient = OpenApiNSwagClientTests.NamingConventions.CamelCase.GeneratedCode.CamelCaseClient;

namespace OpenApiNSwagClientTests.NamingConventions.CamelCase;

public sealed class GeneratedTypesTests
{
    [Fact]
    public void Generated_endpoint_methods_are_named_as_expected()
    {
        _ = nameof(GeneratedClient.GetSupermarketCollectionAsync);
        _ = nameof(GeneratedClient.HeadSupermarketCollectionAsync);
        _ = nameof(GeneratedClient.PostSupermarketAsync);
        _ = nameof(GeneratedClient.GetSupermarketAsync);
        _ = nameof(GeneratedClient.HeadSupermarketAsync);
        _ = nameof(GeneratedClient.PatchSupermarketAsync);
        _ = nameof(GeneratedClient.DeleteSupermarketAsync);
        _ = nameof(GeneratedClient.GetSupermarketBackupStoreManagerAsync);
        _ = nameof(GeneratedClient.HeadSupermarketBackupStoreManagerAsync);
        _ = nameof(GeneratedClient.GetSupermarketBackupStoreManagerRelationshipAsync);
        _ = nameof(GeneratedClient.HeadSupermarketBackupStoreManagerRelationshipAsync);
        _ = nameof(GeneratedClient.PatchSupermarketBackupStoreManagerRelationshipAsync);
        _ = nameof(GeneratedClient.GetSupermarketCashiersAsync);
        _ = nameof(GeneratedClient.HeadSupermarketCashiersAsync);
        _ = nameof(GeneratedClient.GetSupermarketCashiersRelationshipAsync);
        _ = nameof(GeneratedClient.HeadSupermarketCashiersRelationshipAsync);
        _ = nameof(GeneratedClient.PostSupermarketCashiersRelationshipAsync);
        _ = nameof(GeneratedClient.PatchSupermarketCashiersRelationshipAsync);
        _ = nameof(GeneratedClient.DeleteSupermarketCashiersRelationshipAsync);
        _ = nameof(GeneratedClient.GetSupermarketStoreManagerAsync);
        _ = nameof(GeneratedClient.HeadSupermarketStoreManagerAsync);
        _ = nameof(GeneratedClient.GetSupermarketStoreManagerRelationshipAsync);
        _ = nameof(GeneratedClient.HeadSupermarketStoreManagerRelationshipAsync);
        _ = nameof(GeneratedClient.PatchSupermarketStoreManagerRelationshipAsync);

        _ = nameof(GeneratedClient.GetStaffMemberCollectionAsync);
        _ = nameof(GeneratedClient.HeadStaffMemberCollectionAsync);
        _ = nameof(GeneratedClient.PostStaffMemberAsync);
        _ = nameof(GeneratedClient.GetStaffMemberAsync);
        _ = nameof(GeneratedClient.HeadStaffMemberAsync);
        _ = nameof(GeneratedClient.PatchStaffMemberAsync);
        _ = nameof(GeneratedClient.DeleteStaffMemberAsync);

        _ = nameof(GeneratedClient.PostOperationsAsync);
    }

    [Fact]
    public void Generated_top_level_document_types_are_named_as_expected()
    {
        _ = nameof(SupermarketCollectionResponseDocument.Meta);
        _ = nameof(CreateSupermarketRequestDocument.Meta);
        _ = nameof(SupermarketPrimaryResponseDocument.Meta);
        _ = nameof(UpdateSupermarketRequestDocument.Meta);

        _ = nameof(StaffMemberCollectionResponseDocument.Meta);
        _ = nameof(CreateStaffMemberRequestDocument.Meta);
        _ = nameof(StaffMemberPrimaryResponseDocument.Meta);
        _ = nameof(UpdateStaffMemberRequestDocument.Meta);
        _ = nameof(StaffMemberIdentifierCollectionResponseDocument.Meta);
        _ = nameof(StaffMemberIdentifierResponseDocument.Meta);
        _ = nameof(StaffMemberSecondaryResponseDocument.Meta);
        _ = nameof(NullableStaffMemberSecondaryResponseDocument.Meta);
        _ = nameof(NullableStaffMemberIdentifierResponseDocument.Meta);

        _ = nameof(ErrorResponseDocument.Meta);

        _ = nameof(OperationsRequestDocument.Meta);
        _ = nameof(OperationsResponseDocument.Meta);
    }

    [Fact]
    public void Generated_link_types_are_named_as_expected()
    {
        _ = nameof(ResourceTopLevelLinks);
        _ = nameof(ResourceCollectionTopLevelLinks);
        _ = nameof(ResourceIdentifierTopLevelLinks);
        _ = nameof(ResourceIdentifierCollectionTopLevelLinks);
        _ = nameof(ErrorTopLevelLinks);
        _ = nameof(ResourceLinks);
        _ = nameof(RelationshipLinks);
        _ = nameof(ErrorLinks);
    }

    [Fact]
    public void Generated_operation_types_are_named_as_expected()
    {
        _ = nameof(CreateSupermarketOperation.Meta);
        _ = nameof(UpdateSupermarketOperation.Meta);
        _ = nameof(DeleteSupermarketOperation.Meta);
        _ = nameof(UpdateSupermarketBackupStoreManagerRelationshipOperation.Meta);
        _ = nameof(UpdateSupermarketCashiersRelationshipOperation.Meta);
        _ = nameof(AddToSupermarketCashiersRelationshipOperation.Meta);
        _ = nameof(RemoveFromSupermarketCashiersRelationshipOperation.Meta);
        _ = nameof(UpdateSupermarketStoreManagerRelationshipOperation.Meta);

        _ = nameof(CreateStaffMemberOperation.Meta);
        _ = nameof(UpdateStaffMemberOperation.Meta);
        _ = nameof(DeleteStaffMemberOperation.Meta);
    }

    [Fact]
    public void Generated_resource_field_types_are_named_as_expected()
    {
        _ = nameof(AttributesInCreateSupermarketRequest.NameOfCity);
        _ = nameof(AttributesInCreateSupermarketRequest.Kind);
        _ = nameof(AttributesInUpdateSupermarketRequest.NameOfCity);
        _ = nameof(AttributesInUpdateSupermarketRequest.Kind);
        _ = nameof(SupermarketAttributesInResponse.NameOfCity);
        _ = nameof(SupermarketAttributesInResponse.Kind);
        _ = nameof(RelationshipsInCreateSupermarketRequest.StoreManager);
        _ = nameof(RelationshipsInCreateSupermarketRequest.BackupStoreManager);
        _ = nameof(RelationshipsInCreateSupermarketRequest.Cashiers);
        _ = nameof(RelationshipsInUpdateSupermarketRequest.StoreManager);
        _ = nameof(RelationshipsInUpdateSupermarketRequest.BackupStoreManager);
        _ = nameof(RelationshipsInUpdateSupermarketRequest.Cashiers);
        _ = nameof(SupermarketRelationshipsInResponse.StoreManager);
        _ = nameof(SupermarketRelationshipsInResponse.BackupStoreManager);
        _ = nameof(SupermarketRelationshipsInResponse.Cashiers);
        _ = nameof(SupermarketType);

        _ = nameof(AttributesInCreateStaffMemberRequest.Name);
        _ = nameof(AttributesInCreateStaffMemberRequest.Age);
        _ = nameof(AttributesInUpdateStaffMemberRequest.Name);
        _ = nameof(AttributesInUpdateStaffMemberRequest.Age);
        _ = nameof(StaffMemberAttributesInResponse.Name);
        _ = nameof(StaffMemberAttributesInResponse.Age);
    }

    [Fact]
    public void Generated_relationship_container_types_are_named_as_expected()
    {
        _ = nameof(ToOneStaffMemberInRequest.Meta);
        _ = nameof(ToOneStaffMemberInResponse.Meta);
        _ = nameof(NullableToOneStaffMemberInRequest.Meta);
        _ = nameof(NullableToOneStaffMemberInResponse.Meta);
        _ = nameof(ToManyStaffMemberInRequest.Meta);
        _ = nameof(ToManyStaffMemberInResponse.Meta);

        _ = nameof(SupermarketBackupStoreManagerRelationshipIdentifier);
        _ = nameof(SupermarketCashiersRelationshipIdentifier);
        _ = nameof(SupermarketStoreManagerRelationshipIdentifier);
    }

    [Fact]
    public void Generated_relationship_name_enums_are_named_as_expected()
    {
        _ = nameof(SupermarketBackupStoreManagerRelationshipName.BackupStoreManager);
        _ = nameof(SupermarketCashiersRelationshipName.Cashiers);
        _ = nameof(SupermarketStoreManagerRelationshipName.StoreManager);
    }

    [Fact]
    public void Generated_resource_type_enums_are_named_as_expected()
    {
        _ = nameof(SupermarketResourceType.Supermarkets);
        _ = nameof(StaffMemberResourceType.StaffMembers);
        _ = nameof(ResourceType.Supermarkets);
        _ = nameof(ResourceType.StaffMembers);
    }

    [Fact]
    public void Generated_operation_type_enums_are_named_as_expected()
    {
        _ = nameof(AddOperationCode.Add);
        _ = nameof(UpdateOperationCode.Update);
        _ = nameof(RemoveOperationCode.Remove);
    }

    [Fact]
    public void Generated_data_types_are_named_as_expected()
    {
        _ = nameof(DataInResponse.Meta);

        _ = nameof(DataInCreateSupermarketRequest.Meta);
        _ = nameof(DataInUpdateSupermarketRequest.Meta);
        _ = nameof(SupermarketDataInResponse.Meta);

        _ = nameof(SupermarketIdentifierInRequest.Meta);

        _ = nameof(DataInCreateStaffMemberRequest.Meta);
        _ = nameof(DataInUpdateStaffMemberRequest.Meta);
        _ = nameof(StaffMemberDataInResponse.Meta);

        _ = nameof(StaffMemberIdentifierInRequest.Meta);
        _ = nameof(StaffMemberIdentifierInResponse.Meta);
    }

    [Fact]
    public void Generated_predefined_types_are_named_as_expected()
    {
        _ = nameof(Jsonapi.Meta);
        _ = nameof(ErrorObject.Meta);
        _ = nameof(ErrorSource);
        _ = nameof(Meta);

        _ = nameof(AtomicOperation.Meta);
        _ = nameof(AtomicResult.Meta);
    }
}
