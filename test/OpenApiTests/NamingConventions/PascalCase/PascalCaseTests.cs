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

        document.ShouldContainPath("paths./Supermarkets.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketCollection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketCollectionResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceCollectionDocumentSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("jsonapi.$ref").ShouldBeSchemaReferenceId("JsonapiObject");

                linksInResourceCollectionDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInResourceCollectionDocument").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeSchemaReferenceId("SupermarketDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInResourceCollectionDocumentSchemaRefId}.properties").With(propertiesElement =>
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

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceObjectSchemaRefId = propertiesElement.ShouldContainPath("links.$ref").ShouldBeSchemaReferenceId("LinksInResourceObject")
                    .SchemaReferenceId;

                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeSchemaReferenceId("SupermarketResourceType")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.$ref")
                    .ShouldBeSchemaReferenceId("SupermarketAttributesInResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeSchemaReferenceId("SupermarketRelationshipsInResponse").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInResourceObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
            });

            schemasElement.ShouldContainPath($"{primaryResourceTypeSchemaRefId}.enum[0]").With(enumValueElement =>
            {
                enumValueElement.ShouldBeString("Supermarkets");
            });

            schemasElement.ShouldContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("NameOfCity");
                propertiesElement.Should().ContainProperty("Kind");
                propertiesElement.ShouldContainPath("Kind.$ref").ShouldBeSchemaReferenceId("SupermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");

                propertiesElement.ShouldContainPath("StoreManager.$ref").ShouldBeSchemaReferenceId("ToOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.ShouldContainPath("BackupStoreManager.$ref")
                    .ShouldBeSchemaReferenceId("NullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.ShouldContainPath("Cashiers.$ref").ShouldBeSchemaReferenceId("ToManyStaffMemberInResponse");
            });

            string? linksInRelationshipObjectSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInRelationshipObjectSchemaRefId = propertiesElement.ShouldContainPath("links.$ref").ShouldBeSchemaReferenceId("LinksInRelationshipObject")
                    .SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.oneOf[0].$ref")
                    .ShouldBeSchemaReferenceId("StaffMemberIdentifier").SchemaReferenceId;

                propertiesElement.ShouldContainPath("data.oneOf[1].$ref").ShouldBeSchemaReferenceId("NullValue");
            });

            schemasElement.ShouldContainPath($"{linksInRelationshipObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("related");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeSchemaReferenceId("StaffMemberResourceType")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeSchemaReferenceId("StaffMembers");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./Supermarkets/{id}.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketPrimaryResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref").ShouldBeSchemaReferenceId("LinksInResourceDocument")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInResourceDocumentSchemaRefId}.properties").With(propertiesElement =>
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

        document.ShouldContainPath("paths./Supermarkets/{id}/StoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberSecondaryResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeSchemaReferenceId("StaffMemberDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeSchemaReferenceId("StaffMemberAttributesInResponse");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/BackupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketBackupStoreManager");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("NullableStaffMemberSecondaryResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/Cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketCashiers");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
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

        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/StoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberIdentifierResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInResourceIdentifierDocument").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInResourceIdentifierDocumentSchemaRefId}.properties").With(propertiesElement =>
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
        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketBackupStoreManagerRelationship");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
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

        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierCollectionDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierCollectionDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("LinksInResourceIdentifierCollectionDocument").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInResourceIdentifierCollectionDocumentSchemaRefId}.properties").With(propertiesElement =>
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

        document.ShouldContainPath("paths./Supermarkets.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PostSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketPostRequestDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeSchemaReferenceId("SupermarketDataInPostRequest")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeSchemaReferenceId("SupermarketAttributesInPostRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeSchemaReferenceId("SupermarketRelationshipsInPostRequest").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");
                propertiesElement.ShouldContainPath("StoreManager.$ref").ShouldBeSchemaReferenceId("ToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("BackupStoreManager");
                propertiesElement.ShouldContainPath("BackupStoreManager.$ref").ShouldBeSchemaReferenceId("NullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.ShouldContainPath("Cashiers.$ref").ShouldBeSchemaReferenceId("ToManyStaffMemberInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PostSupermarketCashiersRelationship");
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

        document.ShouldContainPath("paths./Supermarkets/{id}.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketPatchRequestDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeSchemaReferenceId("SupermarketDataInPatchRequest")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeSchemaReferenceId("SupermarketAttributesInPatchRequest");
                propertiesElement.ShouldContainPath("relationships.$ref").ShouldBeSchemaReferenceId("SupermarketRelationshipsInPatchRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/StoreManager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarketStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarketBackupStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Delete_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("DeleteSupermarket");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("DeleteSupermarketCashiersRelationship");
            });
        });
    }
}
