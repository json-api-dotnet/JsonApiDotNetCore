using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConventions.CamelCase;

public sealed class CamelCaseTests : IClassFixture<OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
{
    private const string EscapedJsonApiMediaType = "['application/vnd.api+json']";
    private const string EscapedOperationsMediaType = "['application/vnd.api+json; ext=atomic']";

    private readonly OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

    public CamelCaseTests(OpenApiTestContext<CamelCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SupermarketsController>();
        testContext.UseController<StaffMembersController>();
        testContext.UseController<OperationsController>();

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./supermarkets.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketCollection");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("supermarketCollectionResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("jsonapi.allOf[0].$ref").ShouldBeSchemaReferenceId("jsonapi");

                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("resourceCollectionTopLevelLinks").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("dataInSupermarketResponse")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{topLevelLinksSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("describedby");
                propertiesElement.Should().ContainProperty("first");
                propertiesElement.Should().ContainProperty("last");
                propertiesElement.Should().ContainProperty("prev");
                propertiesElement.Should().ContainProperty("next");
            });

            string? resourceLinksSchemaRefId = null;
            string? resourceAttributesInResponseSchemaRefId = null;
            string? resourceRelationshipInResponseSchemaRefId = null;

            string abstractResourceDataSchemaRefId = schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("resourceInResponse").SchemaReferenceId;

            schemasElement.Should().ContainPath($"{abstractResourceDataSchemaRefId}.discriminator.mapping").With(mappingElement =>
            {
                mappingElement.Should().ContainPath("supermarkets").ShouldBeSchemaReferenceId("dataInSupermarketResponse");
                mappingElement.Should().ContainPath("staffMembers").ShouldBeSchemaReferenceId("dataInStaffMemberResponse");
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                resourceLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("resourceLinks")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.Should().ContainPath("attributes.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("attributesInSupermarketResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.Should().ContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("relationshipsInSupermarketResponse").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceLinksSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
            });

            schemasElement.Should().ContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("nameOfCity");
                propertiesElement.Should().ContainProperty("kind");
                propertiesElement.Should().ContainPath("kind.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("storeManager");

                propertiesElement.Should().ContainPath("storeManager.allOf[0].$ref").ShouldBeSchemaReferenceId("toOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.Should().ContainPath("backupStoreManager.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("nullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.Should().ContainPath("cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("toManyStaffMemberInResponse");
            });

            string? relationshipLinksSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.Should().ContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                relationshipLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("relationshipLinks")
                    .SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("staffMemberIdentifierInResponse").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{relationshipLinksSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("related");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.Should().ContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.Should().ContainPath("type.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("staffMemberResourceType").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{relatedResourceTypeSchemaRefId}.enum").With(codeElement =>
            {
                codeElement.Should().ContainArrayElement("staffMembers");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./supermarkets/{id}.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("primarySupermarketResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("resourceTopLevelLinks")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{topLevelLinksSchemaRefId}.properties").With(propertiesElement =>
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

        document.Should().ContainPath("paths./supermarkets/{id}/storeManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("secondaryStaffMemberResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("dataInStaffMemberResponse")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("attributesInStaffMemberResponse");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/backupStoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketBackupStoreManager");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("nullableSecondaryStaffMemberResponseDocument");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/cashiers.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketCashiers");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
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

        document.Should().ContainPath("paths./supermarkets/{id}/relationships/storeManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("staffMemberIdentifierResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("resourceIdentifierTopLevelLinks").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{topLevelLinksSchemaRefId}.properties").With(propertiesElement =>
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
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/backupStoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketBackupStoreManagerRelationship");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
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

        document.Should().ContainPath("paths./supermarkets/{id}/relationships/cashiers.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("getSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("staffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("resourceIdentifierCollectionTopLevelLinks").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{topLevelLinksSchemaRefId}.properties").With(propertiesElement =>
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

        document.Should().ContainPath("paths./supermarkets.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("postSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"requestBody.content{EscapedJsonApiMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("createSupermarketRequestDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("dataInCreateSupermarketRequest").SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("attributesInCreateSupermarketRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.Should().ContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("relationshipsInCreateSupermarketRequest").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("storeManager");
                propertiesElement.Should().ContainPath("storeManager.allOf[0].$ref").ShouldBeSchemaReferenceId("toOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("backupStoreManager");
                propertiesElement.Should().ContainPath("backupStoreManager.allOf[0].$ref").ShouldBeSchemaReferenceId("nullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.Should().ContainPath("cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("toManyStaffMemberInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/cashiers.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("postSupermarketCashiersRelationship");
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

        document.Should().ContainPath("paths./supermarkets/{id}.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("patchSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"requestBody.content{EscapedJsonApiMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("updateSupermarketRequestDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("dataInUpdateSupermarketRequest").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("attributesInUpdateSupermarketRequest");
                propertiesElement.Should().ContainPath("relationships.allOf[0].$ref").ShouldBeSchemaReferenceId("relationshipsInUpdateSupermarketRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/storeManager.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("patchSupermarketStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/backupStoreManager.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("patchSupermarketBackupStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/cashiers.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("patchSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_Delete_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}.delete").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("deleteSupermarket");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/cashiers.delete").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("deleteSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PostOperations_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./operations.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("postOperations");
            });

            getElement.Should().ContainPath($"requestBody.content{EscapedOperationsMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("operationsRequestDocument");

            getElement.Should().ContainPath($"responses.200.content{EscapedOperationsMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("operationsResponseDocument");
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath("addOperationCode.enum").With(codeElement => codeElement.Should().ContainArrayElement("add"));
            schemasElement.Should().ContainPath("updateOperationCode.enum").With(codeElement => codeElement.Should().ContainArrayElement("update"));
            schemasElement.Should().ContainPath("removeOperationCode.enum").With(codeElement => codeElement.Should().ContainArrayElement("remove"));

            schemasElement.Should().ContainPath("atomicOperation.discriminator.mapping").With(mappingElement =>
            {
                mappingElement.Should().ContainPath("addStaffMember").ShouldBeSchemaReferenceId("createStaffMemberOperation");
                mappingElement.Should().ContainPath("addSupermarket").ShouldBeSchemaReferenceId("createSupermarketOperation");
                mappingElement.Should().ContainPath("addToSupermarketCashiers").ShouldBeSchemaReferenceId("addToSupermarketCashiersRelationshipOperation");

                mappingElement.Should().ContainPath("removeFromSupermarketCashiers")
                    .ShouldBeSchemaReferenceId("removeFromSupermarketCashiersRelationshipOperation");

                mappingElement.Should().ContainPath("removeStaffMember").ShouldBeSchemaReferenceId("deleteStaffMemberOperation");
                mappingElement.Should().ContainPath("removeSupermarket").ShouldBeSchemaReferenceId("deleteSupermarketOperation");
                mappingElement.Should().ContainPath("updateStaffMember").ShouldBeSchemaReferenceId("updateStaffMemberOperation");
                mappingElement.Should().ContainPath("updateSupermarket").ShouldBeSchemaReferenceId("updateSupermarketOperation");

                mappingElement.Should().ContainPath("updateSupermarketBackupStoreManager")
                    .ShouldBeSchemaReferenceId("updateSupermarketBackupStoreManagerRelationshipOperation");

                mappingElement.Should().ContainPath("updateSupermarketCashiers").ShouldBeSchemaReferenceId("updateSupermarketCashiersRelationshipOperation");

                mappingElement.Should().ContainPath("updateSupermarketStoreManager")
                    .ShouldBeSchemaReferenceId("updateSupermarketStoreManagerRelationshipOperation");
            });

            schemasElement.Should().ContainPath("createSupermarketOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("dataInCreateSupermarketRequest");
            });

            schemasElement.Should().ContainPath("updateSupermarketOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketIdentifierInRequest");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("dataInUpdateSupermarketRequest");
            });

            schemasElement.Should().ContainPath("deleteSupermarketOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("updateSupermarketStoreManagerRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketStoreManagerRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("updateSupermarketBackupStoreManagerRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketBackupStoreManagerRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("updateSupermarketCashiersRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketCashiersRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("addToSupermarketCashiersRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketCashiersRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("removeFromSupermarketCashiersRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarketCashiersRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("createStaffMemberOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("dataInCreateStaffMemberRequest");
            });

            schemasElement.Should().ContainPath("updateStaffMemberOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("dataInUpdateStaffMemberRequest");
            });

            schemasElement.Should().ContainPath("deleteStaffMemberOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("staffMemberIdentifierInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_error_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.errorResponseDocument");
        document.Should().ContainPath("components.schemas.errorTopLevelLinks");
    }
}
