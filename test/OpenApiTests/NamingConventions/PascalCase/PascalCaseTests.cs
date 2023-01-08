using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConventions.PascalCase;

public sealed class PascalCaseTests
    : IClassFixture<OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext>>
{
    private readonly OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> _testContext;

    public PascalCaseTests(OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SupermarketsController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/NamingConventions/PascalCase";
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketCollection");
            });

            documentSchemaRefId = getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketCollectionResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceCollectionDocumentSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("jsonapi.$ref").ShouldBeSchemaReferenceId("JsonapiObject");

                linksInResourceCollectionDocumentSchemaRefId = propertiesElement.Should().ContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInResourceCollectionDocument").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("SupermarketDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{linksInResourceCollectionDocumentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("describedby");
                propertiesElement.Should().ContainProperty("first");
                propertiesElement.Should().ContainProperty("last");
                propertiesElement.Should().ContainProperty("prev");
                propertiesElement.Should().ContainProperty("next");
            });

            string? linksInResourceObjectSchemaRefId = null;
            string? primaryResourceTypeSchemaRefId = null;
            string? resourceAttributesInResponseSchemaRefId = null;
            string? resourceRelationshipInResponseSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceObjectSchemaRefId = propertiesElement.Should().ContainPath("links.$ref").ShouldBeSchemaReferenceId("LinksInResourceObject")
                    .SchemaReferenceId;

                primaryResourceTypeSchemaRefId = propertiesElement.Should().ContainPath("type.$ref").ShouldBeSchemaReferenceId("SupermarketResourceType")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.Should().ContainPath("attributes.$ref")
                    .ShouldBeSchemaReferenceId("SupermarketAttributesInResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.Should().ContainPath("relationships.$ref")
                    .ShouldBeSchemaReferenceId("SupermarketRelationshipsInResponse").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{linksInResourceObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
            });

            schemasElement.Should().ContainPath($"{primaryResourceTypeSchemaRefId}.enum[0]").With(enumValueElement =>
            {
                enumValueElement.Should().Be("Supermarkets");
            });

            schemasElement.Should().ContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("NameOfCity");
                propertiesElement.Should().ContainProperty("Kind");
                propertiesElement.Should().ContainPath("Kind.$ref").ShouldBeSchemaReferenceId("SupermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");

                propertiesElement.Should().ContainPath("StoreManager.$ref").ShouldBeSchemaReferenceId("ToOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.Should().ContainPath("BackupStoreManager.$ref")
                    .ShouldBeSchemaReferenceId("NullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.Should().ContainPath("Cashiers.$ref").ShouldBeSchemaReferenceId("ToManyStaffMemberInResponse");
            });

            string? linksInRelationshipObjectSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.Should().ContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInRelationshipObjectSchemaRefId = propertiesElement.Should().ContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInRelationshipObject").SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.Should().ContainPath("data.oneOf[0].$ref")
                    .ShouldBeSchemaReferenceId("StaffMemberIdentifier").SchemaReferenceId;

                propertiesElement.Should().ContainPath("data.oneOf[1].$ref").ShouldBeSchemaReferenceId("NullValue");
            });

            schemasElement.Should().ContainPath($"{linksInRelationshipObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("related");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.Should().ContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.Should().ContainPath("type.$ref").ShouldBeSchemaReferenceId("StaffMemberResourceType")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeSchemaReferenceId("StaffMembers");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets/{id}.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketPrimaryResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceDocumentSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceDocumentSchemaRefId = propertiesElement.Should().ContainPath("links.$ref").ShouldBeSchemaReferenceId("LinksInResourceDocument")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{linksInResourceDocumentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("describedby");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_single_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets/{id}/StoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberSecondaryResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.$ref").ShouldBeSchemaReferenceId("StaffMemberDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.$ref").ShouldBeSchemaReferenceId("StaffMemberAttributesInResponse");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/BackupStoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketBackupStoreManager");
            });

            getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("NullableStaffMemberSecondaryResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/Cashiers.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketCashiers");
            });

            getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberCollectionResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/StoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberIdentifierResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierDocumentSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierDocumentSchemaRefId = propertiesElement.Should().ContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInResourceIdentifierDocument").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{linksInResourceIdentifierDocumentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("describedby");
                propertiesElement.Should().ContainProperty("related");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketBackupStoreManagerRelationship");
            });

            getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("NullableStaffMemberIdentifierResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/Cashiers.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.Should().ContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierCollectionDocumentSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierCollectionDocumentSchemaRefId = propertiesElement.Should().ContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInResourceIdentifierCollectionDocument").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{linksInResourceIdentifierCollectionDocumentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("describedby");
                propertiesElement.Should().ContainProperty("related");
                propertiesElement.Should().ContainProperty("first");
                propertiesElement.Should().ContainProperty("last");
                propertiesElement.Should().ContainProperty("prev");
                propertiesElement.Should().ContainProperty("next");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Post_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PostSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketPostRequestDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.$ref").ShouldBeSchemaReferenceId("SupermarketDataInPostRequest")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.$ref").ShouldBeSchemaReferenceId("SupermarketAttributesInPostRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.Should().ContainPath("relationships.$ref")
                    .ShouldBeSchemaReferenceId("SupermarketRelationshipsInPostRequest").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");
                propertiesElement.Should().ContainPath("StoreManager.$ref").ShouldBeSchemaReferenceId("ToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("BackupStoreManager");
                propertiesElement.Should().ContainPath("BackupStoreManager.$ref").ShouldBeSchemaReferenceId("NullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.Should().ContainPath("Cashiers.$ref").ShouldBeSchemaReferenceId("ToManyStaffMemberInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/Cashiers.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PostSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Patch_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./Supermarkets/{id}.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PatchSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketPatchRequestDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.$ref").ShouldBeSchemaReferenceId("SupermarketDataInPatchRequest")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.$ref").ShouldBeSchemaReferenceId("SupermarketAttributesInPatchRequest");
                propertiesElement.Should().ContainPath("relationships.$ref").ShouldBeSchemaReferenceId("SupermarketRelationshipsInPatchRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/StoreManager.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PatchSupermarketStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PatchSupermarketBackupStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/Cashiers.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PatchSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Delete_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}.delete").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("DeleteSupermarket");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/Cashiers.delete").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("DeleteSupermarketCashiersRelationship");
            });
        });
    }
}
