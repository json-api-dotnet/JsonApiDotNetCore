using System.Text.Json;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.NamingConventions.PascalCase;

public sealed class PascalCaseTests : IClassFixture<OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
{
    private const string EscapedJsonApiMediaType = "['application/vnd.api+json; ext=openapi']";
    private const string EscapedOperationsMediaType = "['application/vnd.api+json; ext=atomic; ext=openapi']";

    private readonly OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

    public PascalCaseTests(OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext,
        ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<SupermarketsController>();
        testContext.UseController<StaffMembersController>();
        testContext.UseController<OperationsController>();

        testContext.SetTestOutputHelper(testOutputHelper);
        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
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

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("SupermarketCollectionResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("jsonapi.allOf[0].$ref").ShouldBeSchemaReferenceId("Jsonapi");

                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("ResourceCollectionTopLevelLinks").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("DataInSupermarketResponse")
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
                .ShouldBeSchemaReferenceId("ResourceInResponse").SchemaReferenceId;

            schemasElement.Should().ContainPath($"{abstractResourceDataSchemaRefId}.discriminator.mapping").With(mappingElement =>
            {
                mappingElement.Should().ContainPath("Supermarkets").ShouldBeSchemaReferenceId("DataInSupermarketResponse");
                mappingElement.Should().ContainPath("StaffMembers").ShouldBeSchemaReferenceId("DataInStaffMemberResponse");
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                resourceLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("ResourceLinks")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.Should().ContainPath("attributes.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("AttributesInSupermarketResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.Should().ContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("RelationshipsInSupermarketResponse").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceLinksSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
            });

            schemasElement.Should().ContainPath($"{resourceAttributesInResponseSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("NameOfCity");
                propertiesElement.Should().ContainProperty("Kind");
                propertiesElement.Should().ContainPath("Kind.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceRelationshipInResponseSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");

                propertiesElement.Should().ContainPath("StoreManager.allOf[0].$ref").ShouldBeSchemaReferenceId("ToOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.Should().ContainPath("BackupStoreManager.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("NullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.Should().ContainPath("Cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("ToManyStaffMemberInResponse");
            });

            string? relationshipLinksSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.Should().ContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                relationshipLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("RelationshipLinks")
                    .SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("StaffMemberIdentifierInResponse").SchemaReferenceId;
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
                    .ShouldBeSchemaReferenceId("StaffMemberResourceType").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{relatedResourceTypeSchemaRefId}.enum").With(codeElement =>
            {
                codeElement.Should().ContainArrayElement("StaffMembers");
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

        document.Should().ContainPath("paths./Supermarkets/{id}.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("PrimarySupermarketResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("ResourceTopLevelLinks")
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

        document.Should().ContainPath("paths./Supermarkets/{id}/StoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("SecondaryStaffMemberResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("DataInStaffMemberResponse")
                    .SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("AttributesInStaffMemberResponse");
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

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("NullableSecondaryStaffMemberResponseDocument");
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

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
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

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberIdentifierResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("ResourceIdentifierTopLevelLinks").SchemaReferenceId;
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
        document.Should().ContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("GetSupermarketBackupStoreManagerRelationship");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
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

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("StaffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("ResourceIdentifierCollectionTopLevelLinks").SchemaReferenceId;
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

        document.Should().ContainPath("paths./Supermarkets.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PostSupermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"requestBody.content{EscapedJsonApiMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("CreateSupermarketRequestDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("DataInCreateSupermarketRequest").SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("AttributesInCreateSupermarketRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.Should().ContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("RelationshipsInCreateSupermarketRequest").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");
                propertiesElement.Should().ContainPath("StoreManager.allOf[0].$ref").ShouldBeSchemaReferenceId("ToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("BackupStoreManager");
                propertiesElement.Should().ContainPath("BackupStoreManager.allOf[0].$ref").ShouldBeSchemaReferenceId("NullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.Should().ContainPath("Cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("ToManyStaffMemberInRequest");
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

            documentSchemaRefId = getElement.Should().ContainPath($"requestBody.content{EscapedJsonApiMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("UpdateSupermarketRequestDocument").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("DataInUpdateSupermarketRequest").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("AttributesInUpdateSupermarketRequest");
                propertiesElement.Should().ContainPath("relationships.allOf[0].$ref").ShouldBeSchemaReferenceId("RelationshipsInUpdateSupermarketRequest");
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

    [Fact]
    public async Task Casing_convention_is_applied_to_PostOperations_endpoint()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./Operations.post").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("PostOperations");
            });

            getElement.Should().ContainPath($"requestBody.content{EscapedOperationsMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("OperationsRequestDocument");

            getElement.Should().ContainPath($"responses.200.content{EscapedOperationsMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("OperationsResponseDocument");
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath("AddOperationCode.enum").With(codeElement => codeElement.Should().ContainArrayElement("add"));
            schemasElement.Should().ContainPath("UpdateOperationCode.enum").With(codeElement => codeElement.Should().ContainArrayElement("update"));
            schemasElement.Should().ContainPath("RemoveOperationCode.enum").With(codeElement => codeElement.Should().ContainArrayElement("remove"));

            schemasElement.Should().ContainPath("AtomicOperation.discriminator.mapping").With(mappingElement =>
            {
                foreach (string discriminator in (string[])
                [
                    "CreateStaffMemberOperation",
                    "CreateSupermarketOperation",
                    "AddToSupermarketCashiersRelationshipOperation",
                    "RemoveFromSupermarketCashiersRelationshipOperation",
                    "DeleteStaffMemberOperation",
                    "DeleteSupermarketOperation",
                    "UpdateStaffMemberOperation",
                    "UpdateSupermarketOperation",
                    "UpdateSupermarketBackupStoreManagerRelationshipOperation",
                    "UpdateSupermarketCashiersRelationshipOperation",
                    "UpdateSupermarketStoreManagerRelationshipOperation"
                ])
                {
                    mappingElement.Should().ContainPath(discriminator).ShouldBeSchemaReferenceId(discriminator);
                }
            });

            schemasElement.Should().ContainPath("CreateSupermarketOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("DataInCreateSupermarketRequest");
            });

            schemasElement.Should().ContainPath("UpdateSupermarketOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketIdentifierInRequest");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("DataInUpdateSupermarketRequest");
            });

            schemasElement.Should().ContainPath("DeleteSupermarketOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("UpdateSupermarketStoreManagerRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketStoreManagerRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("UpdateSupermarketBackupStoreManagerRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketBackupStoreManagerRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("UpdateSupermarketCashiersRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketCashiersRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("AddToSupermarketCashiersRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketCashiersRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("RemoveFromSupermarketCashiersRelationshipOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("SupermarketCashiersRelationshipIdentifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
            });

            schemasElement.Should().ContainPath("CreateStaffMemberOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("DataInCreateStaffMemberRequest");
            });

            schemasElement.Should().ContainPath("UpdateStaffMemberOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("DataInUpdateStaffMemberRequest");
            });

            schemasElement.Should().ContainPath("DeleteStaffMemberOperation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("StaffMemberIdentifierInRequest");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_error_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.ErrorResponseDocument");
        document.Should().ContainPath("components.schemas.ErrorTopLevelLinks");
    }
}
