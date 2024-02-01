using OpenApiClientTests.NamingConventions.PascalCase.GeneratedCode;
using Xunit;
using GeneratedClient = OpenApiClientTests.NamingConventions.PascalCase.GeneratedCode.PascalCaseClient;

namespace OpenApiClientTests.NamingConventions.PascalCase;

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
    }

    [Fact]
    public void Generated_top_level_document_types_are_named_as_expected()
    {
        _ = nameof(SupermarketCollectionResponseDocument);
        _ = nameof(SupermarketPostRequestDocument);
        _ = nameof(SupermarketPrimaryResponseDocument);
        _ = nameof(SupermarketPatchRequestDocument);

        _ = nameof(StaffMemberCollectionResponseDocument);
        _ = nameof(StaffMemberPostRequestDocument);
        _ = nameof(StaffMemberPrimaryResponseDocument);
        _ = nameof(StaffMemberPatchRequestDocument);
        _ = nameof(StaffMemberIdentifierCollectionResponseDocument);
        _ = nameof(StaffMemberIdentifierResponseDocument);
        _ = nameof(StaffMemberSecondaryResponseDocument);
        _ = nameof(NullableStaffMemberSecondaryResponseDocument);
        _ = nameof(NullableStaffMemberIdentifierResponseDocument);
    }

    [Fact]
    public void Generated_link_types_are_named_as_expected()
    {
        _ = nameof(LinksInResourceCollectionDocument);
        _ = nameof(LinksInResourceDocument);
        _ = nameof(LinksInResourceIdentifierCollectionDocument);
        _ = nameof(LinksInResourceIdentifierDocument);
        _ = nameof(LinksInResourceData);
        _ = nameof(LinksInRelationship);
    }

    [Fact]
    public void Generated_resource_field_types_are_named_as_expected()
    {
        _ = nameof(SupermarketAttributesInPostRequest.NameOfCity);
        _ = nameof(SupermarketAttributesInPostRequest.Kind);
        _ = nameof(SupermarketAttributesInPatchRequest.NameOfCity);
        _ = nameof(SupermarketAttributesInPatchRequest.Kind);
        _ = nameof(SupermarketAttributesInResponse.NameOfCity);
        _ = nameof(SupermarketAttributesInResponse.Kind);
        _ = nameof(SupermarketRelationshipsInPostRequest.StoreManager);
        _ = nameof(SupermarketRelationshipsInPostRequest.BackupStoreManager);
        _ = nameof(SupermarketRelationshipsInPostRequest.Cashiers);
        _ = nameof(SupermarketRelationshipsInPatchRequest.StoreManager);
        _ = nameof(SupermarketRelationshipsInPatchRequest.BackupStoreManager);
        _ = nameof(SupermarketRelationshipsInPatchRequest.Cashiers);
        _ = nameof(SupermarketRelationshipsInResponse.StoreManager);
        _ = nameof(SupermarketRelationshipsInResponse.BackupStoreManager);
        _ = nameof(SupermarketRelationshipsInResponse.Cashiers);
        _ = nameof(SupermarketType);

        _ = nameof(StaffMemberAttributesInPostRequest.Name);
        _ = nameof(StaffMemberAttributesInPostRequest.Age);
        _ = nameof(StaffMemberAttributesInPatchRequest.Name);
        _ = nameof(StaffMemberAttributesInPatchRequest.Age);
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
    }

    [Fact]
    public void Generated_resource_type_enums_are_named_as_expected()
    {
        _ = nameof(SupermarketResourceType.Supermarkets);
        _ = nameof(StaffMemberResourceType.StaffMembers);
    }

    [Fact]
    public void Generated_data_types_are_named_as_expected()
    {
        _ = nameof(SupermarketDataInPostRequest);
        _ = nameof(SupermarketDataInPatchRequest);
        _ = nameof(SupermarketDataInResponse);

        _ = nameof(StaffMemberDataInPostRequest);
        _ = nameof(StaffMemberDataInPatchRequest);
        _ = nameof(StaffMemberDataInResponse);

        _ = nameof(DataInResponse);

        _ = nameof(StaffMemberIdentifier);
    }

    [Fact]
    public void Generated_code_is_named_as_expected()
    {
        _ = nameof(Jsonapi);
    }
}
