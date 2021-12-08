using FluentAssertions;
using OpenApiClientTests.NamingConvention.CamelCase.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.NamingConvention.CamelCase;

public sealed class GeneratedTypesTests
{
    [Fact]
    public void Generated_code_is_named_as_expected()
    {
        nameof(CamelCaseClient.GetSupermarketCollectionAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketCollectionAsync).Should().NotBeNull();
        nameof(CamelCaseClient.PostSupermarketAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketAsync).Should().NotBeNull();
        nameof(CamelCaseClient.PatchSupermarketAsync).Should().NotBeNull();
        nameof(CamelCaseClient.DeleteSupermarketAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketBackupStoreManagerAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketBackupStoreManagerAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.PatchSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketCashiersAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketCashiersAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.PostSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.PatchSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.DeleteSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketStoreManagerAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketStoreManagerAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(CamelCaseClient.PatchSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();

        nameof(SupermarketCollectionResponseDocument).Should().NotBeNull();
        nameof(LinksInResourceCollectionDocument).Should().NotBeNull();
        nameof(JsonapiObject).Should().NotBeNull();
        nameof(SupermarketDataInResponse).Should().NotBeNull();
        nameof(SupermarketResourceType).Should().NotBeNull();
        nameof(SupermarketAttributesInResponse.NameOfCity).Should().NotBeNull();
        nameof(SupermarketRelationshipsInResponse.StoreManager).Should().NotBeNull();
        nameof(SupermarketRelationshipsInResponse.BackupStoreManager).Should().NotBeNull();
        nameof(LinksInResourceObject).Should().NotBeNull();
        nameof(SupermarketType).Should().NotBeNull();
        nameof(CamelCaseClient.GetSupermarketAsync).Should().NotBeNull();
        nameof(ToOneStaffMemberInResponse).Should().NotBeNull();
        nameof(NullableToOneStaffMemberInResponse).Should().NotBeNull();
        nameof(ToManyStaffMemberInResponse).Should().NotBeNull();
        nameof(LinksInRelationshipObject).Should().NotBeNull();
        nameof(StaffMemberIdentifier).Should().NotBeNull();
        nameof(StaffMemberResourceType.StaffMembers).Should().NotBeNull();
        nameof(SupermarketPrimaryResponseDocument).Should().NotBeNull();
        nameof(LinksInResourceDocument).Should().NotBeNull();
        nameof(StaffMemberSecondaryResponseDocument).Should().NotBeNull();
        nameof(StaffMemberDataInResponse).Should().NotBeNull();
        nameof(StaffMemberAttributesInResponse).Should().NotBeNull();
        nameof(NullableStaffMemberSecondaryResponseDocument).Should().NotBeNull();
        nameof(StaffMemberCollectionResponseDocument).Should().NotBeNull();
        nameof(StaffMemberIdentifierResponseDocument).Should().NotBeNull();
        nameof(LinksInResourceIdentifierDocument).Should().NotBeNull();
        nameof(NullableStaffMemberIdentifierResponseDocument).Should().NotBeNull();
        nameof(StaffMemberIdentifierCollectionResponseDocument).Should().NotBeNull();
        nameof(LinksInResourceIdentifierCollectionDocument).Should().NotBeNull();
        nameof(SupermarketPostRequestDocument).Should().NotBeNull();
        nameof(SupermarketDataInPostRequest).Should().NotBeNull();
        nameof(SupermarketAttributesInPostRequest).Should().NotBeNull();
        nameof(SupermarketRelationshipsInPostRequest).Should().NotBeNull();
        nameof(ToOneStaffMemberInRequest).Should().NotBeNull();
        nameof(NullableToOneStaffMemberInRequest).Should().NotBeNull();
        nameof(ToManyStaffMemberInRequest).Should().NotBeNull();
        nameof(SupermarketPatchRequestDocument).Should().NotBeNull();
        nameof(SupermarketDataInPatchRequest).Should().NotBeNull();
        nameof(SupermarketAttributesInPatchRequest).Should().NotBeNull();
        nameof(SupermarketRelationshipsInPatchRequest).Should().NotBeNull();
    }
}
