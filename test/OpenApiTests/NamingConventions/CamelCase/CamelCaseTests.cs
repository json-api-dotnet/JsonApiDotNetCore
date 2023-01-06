using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConventions.CamelCase;

public sealed class CamelCaseTests : IClassFixture<OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext>>
{
    private readonly OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> _testContext;

    public CamelCaseTests(OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SupermarketsController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/NamingConventions/CamelCase";
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCollection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("supermarketCollectionResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceCollectionDocumentSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("jsonapi.allOf[0].$ref").ShouldBeSchemaReferenceId("jsonapiObject");

                linksInResourceCollectionDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("linksInResourceCollectionDocument").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeSchemaReferenceId("supermarketDataInResponse")
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
                linksInResourceObjectSchemaRefId = propertiesElement.ShouldContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("linksInResourceObject")
                    .SchemaReferenceId;

                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketResourceType")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("supermarketAttributesInResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("supermarketRelationshipsInResponse").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInResourceObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
            });

            schemasElement.ShouldContainPath($"{primaryResourceTypeSchemaRefId}.enum[0]").With(enumValueElement =>
            {
                enumValueElement.ShouldBeString("supermarkets");
            });

            schemasElement.ShouldContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("nameOfCity");
                propertiesElement.Should().ContainProperty("kind");
                propertiesElement.ShouldContainPath("kind.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("storeManager");

                propertiesElement.ShouldContainPath("storeManager.allOf[0].$ref").ShouldBeSchemaReferenceId("toOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.ShouldContainPath("backupStoreManager.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("nullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("toManyStaffMemberInResponse");
            });

            string? linksInRelationshipObjectSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInRelationshipObjectSchemaRefId = propertiesElement.ShouldContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("linksInRelationshipObject").SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("staffMemberIdentifier").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{linksInRelationshipObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("related");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberResourceType")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeSchemaReferenceId("staffMembers");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("supermarketPrimaryResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("linksInResourceDocument").SchemaReferenceId;
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

        document.ShouldContainPath("paths./supermarkets/{id}/storeManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staffMemberSecondaryResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberAttributesInResponse");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/backupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketBackupStoreManager");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("nullableStaffMemberSecondaryResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCashiers");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staffMemberCollectionResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/storeManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staffMemberIdentifierResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("linksInResourceIdentifierDocument").SchemaReferenceId;
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
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/backupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketBackupStoreManagerRelationship");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("nullableStaffMemberIdentifierResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierCollectionDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierCollectionDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("linksInResourceIdentifierCollectionDocument").SchemaReferenceId;
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

        document.ShouldContainPath("paths./supermarkets.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("postSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("supermarketPostRequestDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketDataInPostRequest")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketAttributesInPostRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("supermarketRelationshipsInPostRequest").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("storeManager");
                propertiesElement.ShouldContainPath("storeManager.allOf[0].$ref").ShouldBeSchemaReferenceId("toOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("backupStoreManager");
                propertiesElement.ShouldContainPath("backupStoreManager.allOf[0].$ref").ShouldBeSchemaReferenceId("nullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("toManyStaffMemberInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

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
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patchSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("supermarketPatchRequestDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketDataInPatchRequest")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketAttributesInPatchRequest");
                propertiesElement.ShouldContainPath("relationships.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketRelationshipsInPatchRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

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
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

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
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

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
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

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
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

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
