using OpenApiNSwagClientTests.NamingConventions.PascalCase.GeneratedCode;
using Xunit;
using GeneratedClient = OpenApiNSwagClientTests.NamingConventions.PascalCase.GeneratedCode.PascalCaseClient;

namespace OpenApiNSwagClientTests.NamingConventions.PascalCase;

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
        _ = nameof(SupermarketCollectionResponseDocument);
        _ = nameof(CreateSupermarketRequestDocument);
        _ = nameof(SupermarketPrimaryResponseDocument);
        _ = nameof(UpdateSupermarketRequestDocument);

        _ = nameof(StaffMemberCollectionResponseDocument);
        _ = nameof(CreateStaffMemberRequestDocument);
        _ = nameof(StaffMemberPrimaryResponseDocument);
        _ = nameof(UpdateStaffMemberRequestDocument);
        _ = nameof(StaffMemberIdentifierCollectionResponseDocument);
        _ = nameof(StaffMemberIdentifierResponseDocument);
        _ = nameof(StaffMemberSecondaryResponseDocument);
        _ = nameof(NullableStaffMemberSecondaryResponseDocument);
        _ = nameof(NullableStaffMemberIdentifierResponseDocument);

        _ = nameof(ErrorResponseDocument);

        _ = nameof(OperationsRequestDocument);
        _ = nameof(OperationsResponseDocument);
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
        _ = nameof(CreateSupermarketOperation);
        _ = nameof(UpdateSupermarketOperation);
        _ = nameof(DeleteSupermarketOperation);
        _ = nameof(UpdateSupermarketBackupStoreManagerRelationshipOperation);
        _ = nameof(UpdateSupermarketCashiersRelationshipOperation);
        _ = nameof(AddToSupermarketCashiersRelationshipOperation);
        _ = nameof(RemoveFromSupermarketCashiersRelationshipOperation);
        _ = nameof(UpdateSupermarketStoreManagerRelationshipOperation);

        _ = nameof(CreateStaffMemberOperation);
        _ = nameof(UpdateStaffMemberOperation);
        _ = nameof(DeleteStaffMemberOperation);
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
        _ = nameof(ToOneStaffMemberInRequest);
        _ = nameof(ToOneStaffMemberInResponse);
        _ = nameof(NullableToOneStaffMemberInRequest);
        _ = nameof(NullableToOneStaffMemberInResponse);
        _ = nameof(ToManyStaffMemberInRequest);
        _ = nameof(ToManyStaffMemberInResponse);

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
        _ = nameof(DataInResponse);

        _ = nameof(DataInCreateSupermarketRequest);
        _ = nameof(DataInUpdateSupermarketRequest);
        _ = nameof(SupermarketDataInResponse);

        _ = nameof(SupermarketIdentifierInRequest);

        _ = nameof(DataInCreateStaffMemberRequest);
        _ = nameof(DataInUpdateStaffMemberRequest);
        _ = nameof(StaffMemberDataInResponse);

        _ = nameof(StaffMemberIdentifierInRequest);
        _ = nameof(StaffMemberIdentifierInResponse);
    }

    [Fact]
    public void Generated_predefined_types_are_named_as_expected()
    {
        _ = nameof(Jsonapi);
        _ = nameof(ErrorObject);
        _ = nameof(ErrorSource);
        _ = nameof(Meta);

        _ = nameof(AtomicOperation);
        _ = nameof(AtomicResult);
    }
}
