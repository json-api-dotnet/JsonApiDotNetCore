using FluentAssertions;
using OpenApiClientTests.NamingConvention.PascalCase.GeneratedCode;
using Xunit;

namespace OpenApiClientTests.NamingConvention.PascalCase;

public sealed class GeneratedTypesTests
{
    [Fact]
    public void Generated_code_is_named_as_expected()
    {
        nameof(PascalCaseClient.GetSupermarketCollectionAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketCollectionAsync).Should().NotBeNull();
        nameof(PascalCaseClient.PostSupermarketAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketAsync).Should().NotBeNull();
        nameof(PascalCaseClient.PatchSupermarketAsync).Should().NotBeNull();
        nameof(PascalCaseClient.DeleteSupermarketAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketBackupStoreManagerAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketBackupStoreManagerAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.PatchSupermarketBackupStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketCashiersAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketCashiersAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.PostSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.PatchSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.DeleteSupermarketCashiersRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketStoreManagerAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketStoreManagerAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.GetSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();
        nameof(PascalCaseClient.PatchSupermarketStoreManagerRelationshipAsync).Should().NotBeNull();

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
        nameof(PascalCaseClient.GetSupermarketAsync).Should().NotBeNull();
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
