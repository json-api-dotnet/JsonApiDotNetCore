using FluentAssertions;
using OpenApiClientTests.NamingConvention.KebabCase.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.NamingConvention.KebabCase
{
    public sealed class GeneratedTypesTests
    {
        [Fact]
        public void Generated_code_is_named_as_expected()
        {
            nameof(KebabCaseClient.GetSupermarketCollectionAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketCollectionAsync).Should().NotBeNull();
            nameof(KebabCaseClient.PostSupermarketAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketAsync).Should().NotBeNull();
            nameof(KebabCaseClient.PatchSupermarketAsync).Should().NotBeNull();
            nameof(KebabCaseClient.DeleteSupermarketAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketBackupStoreManagerAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketBackupStoreManagerAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.PatchSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketCashiersAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketCashiersAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketCashiersRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketCashiersRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.PostSupermarketCashiersRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.PatchSupermarketCashiersRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.DeleteSupermarketCashiersRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketStoreManagerAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketStoreManagerAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.GetSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();
            nameof(KebabCaseClient.PatchSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();

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
            nameof(KebabCaseClient.GetSupermarketAsync).Should().NotBeNull();
            nameof(ToOneStaffMemberInResponse).Should().NotBeNull();
            nameof(NullableToOneStaffMemberInResponse).Should().NotBeNull();
            nameof(ToManyStaffMemberInResponse).Should().NotBeNull();
            nameof(LinksInRelationshipObject).Should().NotBeNull();
            nameof(StaffMemberIdentifier).Should().NotBeNull();
            nameof(StaffMemberResourceType).Should().NotBeNull();
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
}
