using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConventions.CamelCase;

public sealed class CamelCaseTests : OpenApiTestSuite<CamelCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext>
{
    public CamelCaseTests(OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> testContext)
        : base(testContext)
    {
        UseController<SupermarketsController>();
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCollection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketCollectionResponseDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInResourceCollectionDocument");
                propertiesElement.ShouldContainPath("jsonapi.$ref").ShouldBeReferenceSchemaId("jsonapiObject");

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeReferenceSchemaId("supermarketDataInResponse")
                    .ReferenceSchemaId;
            });

            string? resourceAttributesInResponseSchemaRefId = null;
            string? resourceRelationshipInResponseSchemaRefId = null;
            string? primaryResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("supermarketResourceType")
                    .ReferenceSchemaId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.$ref")
                    .ShouldBeReferenceSchemaId("supermarketAttributesInResponse").ReferenceSchemaId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("supermarketRelationshipsInResponse").ReferenceSchemaId;

                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInResourceObject");
            });

            schemasElement.ShouldContainPath($"{primaryResourceTypeSchemaRefId}.enum[0]").With(enumValueElement =>
            {
                enumValueElement.ShouldBeString("supermarkets");
            });

            schemasElement.ShouldContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("nameOfCity");
                propertiesElement.Should().ContainProperty("kind");
                propertiesElement.ShouldContainPath("kind.$ref").ShouldBeReferenceSchemaId("supermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("storeManager");

                propertiesElement.ShouldContainPath("storeManager.$ref").ShouldBeReferenceSchemaId("toOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.ShouldContainPath("backupStoreManager.$ref")
                    .ShouldBeReferenceSchemaId("nullableToOneStaffMemberInResponse").ReferenceSchemaId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeReferenceSchemaId("toManyStaffMemberInResponse");
            });

            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInRelationshipObject");

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.oneOf[0].$ref")
                    .ShouldBeReferenceSchemaId("staffMemberIdentifier").ReferenceSchemaId;

                propertiesElement.ShouldContainPath("data.oneOf[1].$ref").ShouldBeReferenceSchemaId("nullValue");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("staffMemberResourceType")
                    .ReferenceSchemaId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeReferenceSchemaId("staffMembers");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketPrimaryResponseDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInResourceDocument");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_single_resource()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/storeManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberSecondaryResponseDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("staffMemberDataInResponse")
                    .ReferenceSchemaId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("staffMemberAttributesInResponse");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/backupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketBackupStoreManager");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("nullableStaffMemberSecondaryResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCashiers");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberCollectionResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/storeManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberIdentifierResponseDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInResourceIdentifierDocument");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/backupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketBackupStoreManagerRelationship");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("nullableStaffMemberIdentifierResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberIdentifierCollectionResponseDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInResourceIdentifierCollectionDocument");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Post_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("postSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketPostRequestDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("supermarketDataInPostRequest")
                    .ReferenceSchemaId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("supermarketAttributesInPostRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("supermarketRelationshipsInPostRequest").ReferenceSchemaId;
            });

            schemasElement.ShouldContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("storeManager");
                propertiesElement.ShouldContainPath("storeManager.$ref").ShouldBeReferenceSchemaId("toOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("backupStoreManager");
                propertiesElement.ShouldContainPath("backupStoreManager.$ref").ShouldBeReferenceSchemaId("nullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeReferenceSchemaId("toManyStaffMemberInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("postSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Patch_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patchSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketPatchRequestDocument").ReferenceSchemaId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("supermarketDataInPatchRequest")
                    .ReferenceSchemaId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("supermarketAttributesInPatchRequest");
                propertiesElement.ShouldContainPath("relationships.$ref").ShouldBeReferenceSchemaId("supermarketRelationshipsInPatchRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/storeManager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patchSupermarketStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/backupStoreManager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patchSupermarketBackupStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patchSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Delete_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("deleteSupermarket");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Act
        JsonElement document = await GetDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("deleteSupermarketCashiersRelationship");
            });
        });
    }
}
