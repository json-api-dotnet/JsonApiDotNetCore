using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConventions.KebabCase;

public sealed class KebabCaseTests : IClassFixture<OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
{
    private const string EscapedJsonApiMediaType = "['application/vnd.api+json']";
    private const string EscapedOperationsMediaType = "['application/vnd.api+json; ext=atomic']";

    private readonly OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

    public KebabCaseTests(OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
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
                operationElement.Should().Be("get-supermarket-collection");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("supermarket-collection-response-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("jsonapi.allOf[0].$ref").ShouldBeSchemaReferenceId("jsonapi");

                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("resource-collection-top-level-links").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("data-in-supermarket-response")
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
                .ShouldBeSchemaReferenceId("resource-in-response").SchemaReferenceId;

            schemasElement.Should().ContainPath($"{abstractResourceDataSchemaRefId}.discriminator.mapping").With(mappingElement =>
            {
                mappingElement.Should().ContainPath("supermarkets").ShouldBeSchemaReferenceId("data-in-supermarket-response");
                mappingElement.Should().ContainPath("staff-members").ShouldBeSchemaReferenceId("data-in-staff-member-response");
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                resourceLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("resource-links")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.Should().ContainPath("attributes.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("attributes-in-supermarket-response").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.Should().ContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("relationships-in-supermarket-response").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceLinksSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
            });

            schemasElement.Should().ContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("name-of-city");
                propertiesElement.Should().ContainProperty("kind");
                propertiesElement.Should().ContainPath("kind.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-type");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("store-manager");

                propertiesElement.Should().ContainPath("store-manager.allOf[0].$ref").ShouldBeSchemaReferenceId("to-one-staff-member-in-response");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.Should().ContainPath("backup-store-manager.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("nullable-to-one-staff-member-in-response").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.Should().ContainPath("cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("to-many-staff-member-in-response");
            });

            string? relationshipLinksSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.Should().ContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                relationshipLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("relationship-links")
                    .SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("staff-member-identifier-in-response").SchemaReferenceId;
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
                    .ShouldBeSchemaReferenceId("staff-member-resource-type").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{relatedResourceTypeSchemaRefId}.enum").With(codeElement =>
            {
                codeElement.Should().ContainArrayElement("staff-members");
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
                operationElement.Should().Be("get-supermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("primary-supermarket-response-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref").ShouldBeSchemaReferenceId("resource-top-level-links")
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

        document.Should().ContainPath("paths./supermarkets/{id}/store-manager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("get-supermarket-store-manager");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("secondary-staff-member-response-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("data-in-staff-member-response").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("attributes-in-staff-member-response");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/backup-store-manager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("get-supermarket-backup-store-manager");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("nullable-secondary-staff-member-response-document");
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
                operationElement.Should().Be("get-supermarket-cashiers");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("staff-member-collection-response-document");
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        string? documentSchemaRefId = null;

        document.Should().ContainPath("paths./supermarkets/{id}/relationships/store-manager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("get-supermarket-store-manager-relationship");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("staff-member-identifier-response-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("resource-identifier-top-level-links").SchemaReferenceId;
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
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/backup-store-manager.get").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("get-supermarket-backup-store-manager-relationship");
            });

            getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("nullable-staff-member-identifier-response-document");
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
                operationElement.Should().Be("get-supermarket-cashiers-relationship");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"responses.200.content{EscapedJsonApiMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("staff-member-identifier-collection-response-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? topLevelLinksSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                topLevelLinksSchemaRefId = propertiesElement.Should().ContainPath("links.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("resource-identifier-collection-top-level-links").SchemaReferenceId;
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
                operationElement.Should().Be("post-supermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"requestBody.content{EscapedJsonApiMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("create-supermarket-request-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("data-in-create-supermarket-request").SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("attributes-in-create-supermarket-request");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.Should().ContainPath("relationships.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("relationships-in-create-supermarket-request").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("store-manager");
                propertiesElement.Should().ContainPath("store-manager.allOf[0].$ref").ShouldBeSchemaReferenceId("to-one-staff-member-in-request");

                propertiesElement.Should().ContainProperty("backup-store-manager");

                propertiesElement.Should().ContainPath("backup-store-manager.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("nullable-to-one-staff-member-in-request");

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.Should().ContainPath("cashiers.allOf[0].$ref").ShouldBeSchemaReferenceId("to-many-staff-member-in-request");
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
                operationElement.Should().Be("post-supermarket-cashiers-relationship");
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
                operationElement.Should().Be("patch-supermarket");
            });

            documentSchemaRefId = getElement.Should().ContainPath($"requestBody.content{EscapedJsonApiMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("update-supermarket-request-document").SchemaReferenceId;
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.Should().ContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.Should().ContainPath("data.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("data-in-update-supermarket-request").SchemaReferenceId;
            });

            schemasElement.Should().ContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("attributes.allOf[0].$ref").ShouldBeSchemaReferenceId("attributes-in-update-supermarket-request");
                propertiesElement.Should().ContainPath("relationships.allOf[0].$ref").ShouldBeSchemaReferenceId("relationships-in-update-supermarket-request");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/store-manager.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("patch-supermarket-store-manager-relationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("paths./supermarkets/{id}/relationships/backup-store-manager.patch").With(getElement =>
        {
            getElement.Should().ContainPath("operationId").With(operationElement =>
            {
                operationElement.Should().Be("patch-supermarket-backup-store-manager-relationship");
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
                operationElement.Should().Be("patch-supermarket-cashiers-relationship");
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
                operationElement.Should().Be("delete-supermarket");
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
                operationElement.Should().Be("delete-supermarket-cashiers-relationship");
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
                operationElement.Should().Be("post-operations");
            });

            getElement.Should().ContainPath($"requestBody.content{EscapedOperationsMediaType}.schema.allOf[0].$ref")
                .ShouldBeSchemaReferenceId("operations-request-document");

            getElement.Should().ContainPath($"responses.200.content{EscapedOperationsMediaType}.schema.$ref")
                .ShouldBeSchemaReferenceId("operations-response-document");
        });

        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath("add-operation-code.enum").With(codeElement => codeElement.Should().ContainArrayElement("add"));
            schemasElement.Should().ContainPath("update-operation-code.enum").With(codeElement => codeElement.Should().ContainArrayElement("update"));
            schemasElement.Should().ContainPath("remove-operation-code.enum").With(codeElement => codeElement.Should().ContainArrayElement("remove"));

            schemasElement.Should().ContainPath("atomic-operation.discriminator.mapping").With(mappingElement =>
            {
                mappingElement.Should().ContainPath("add-staff-member").ShouldBeSchemaReferenceId("create-staff-member-operation");
                mappingElement.Should().ContainPath("add-supermarket").ShouldBeSchemaReferenceId("create-supermarket-operation");

                mappingElement.Should().ContainPath("add-to-supermarket-cashiers")
                    .ShouldBeSchemaReferenceId("add-to-supermarket-cashiers-relationship-operation");

                mappingElement.Should().ContainPath("remove-from-supermarket-cashiers")
                    .ShouldBeSchemaReferenceId("remove-from-supermarket-cashiers-relationship-operation");

                mappingElement.Should().ContainPath("remove-staff-member").ShouldBeSchemaReferenceId("delete-staff-member-operation");
                mappingElement.Should().ContainPath("remove-supermarket").ShouldBeSchemaReferenceId("delete-supermarket-operation");
                mappingElement.Should().ContainPath("update-staff-member").ShouldBeSchemaReferenceId("update-staff-member-operation");
                mappingElement.Should().ContainPath("update-supermarket").ShouldBeSchemaReferenceId("update-supermarket-operation");

                mappingElement.Should().ContainPath("update-supermarket-backup-store-manager")
                    .ShouldBeSchemaReferenceId("update-supermarket-backup-store-manager-relationship-operation");

                mappingElement.Should().ContainPath("update-supermarket-cashiers")
                    .ShouldBeSchemaReferenceId("update-supermarket-cashiers-relationship-operation");

                mappingElement.Should().ContainPath("update-supermarket-store-manager")
                    .ShouldBeSchemaReferenceId("update-supermarket-store-manager-relationship-operation");
            });

            schemasElement.Should().ContainPath("create-supermarket-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("data-in-create-supermarket-request");
            });

            schemasElement.Should().ContainPath("update-supermarket-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-identifier-in-request");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("data-in-update-supermarket-request");
            });

            schemasElement.Should().ContainPath("delete-supermarket-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-identifier-in-request");
            });

            schemasElement.Should().ContainPath("update-supermarket-store-manager-relationship-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-store-manager-relationship-identifier");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
            });

            schemasElement.Should().ContainPath("update-supermarket-backup-store-manager-relationship-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref")
                    .ShouldBeSchemaReferenceId("supermarket-backup-store-manager-relationship-identifier");

                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
            });

            schemasElement.Should().ContainPath("update-supermarket-cashiers-relationship-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-cashiers-relationship-identifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
            });

            schemasElement.Should().ContainPath("add-to-supermarket-cashiers-relationship-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-cashiers-relationship-identifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
            });

            schemasElement.Should().ContainPath("remove-from-supermarket-cashiers-relationship-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("supermarket-cashiers-relationship-identifier");
                propertiesElement.Should().ContainPath("data.items.$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
            });

            schemasElement.Should().ContainPath("create-staff-member-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("data-in-create-staff-member-request");
            });

            schemasElement.Should().ContainPath("update-staff-member-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
                propertiesElement.Should().ContainPath("data.allOf[0].$ref").ShouldBeSchemaReferenceId("data-in-update-staff-member-request");
            });

            schemasElement.Should().ContainPath("delete-staff-member-operation.allOf[1].properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainPath("ref.allOf[0].$ref").ShouldBeSchemaReferenceId("staff-member-identifier-in-request");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_error_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.error-response-document");
        document.Should().ContainPath("components.schemas.error-top-level-links");
    }
}
