using System.Text.Json;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConventions.KebabCase;

public sealed class PascalCaseTests
    : IClassFixture<OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext>>
{
    private readonly OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> _testContext;

    public PascalCaseTests(OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionsDbContext>, NamingConventionsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SupermarketsController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/NamingConventions/KebabCase";
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
                operationElement.ShouldBeString("get-supermarket-collection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("supermarket-collection-response-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceCollectionDocumentSchemaRefId = null;
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("jsonapi.$ref").ShouldBeSchemaReferenceId("jsonapi-object");

                linksInResourceCollectionDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("links-in-resource-collection-document").SchemaReferenceId;

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeSchemaReferenceId("supermarket-data-in-response")
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
                linksInResourceObjectSchemaRefId = propertiesElement.ShouldContainPath("links.$ref").ShouldBeSchemaReferenceId("links-in-resource-object")
                    .SchemaReferenceId;

                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeSchemaReferenceId("supermarket-resource-type")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.$ref")
                    .ShouldBeSchemaReferenceId("supermarket-attributes-in-response").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeSchemaReferenceId("supermarket-relationships-in-response").SchemaReferenceId;
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
                propertiesElement.Should().ContainProperty("name-of-city");
                propertiesElement.Should().ContainProperty("kind");
                propertiesElement.ShouldContainPath("kind.$ref").ShouldBeSchemaReferenceId("supermarket-type");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("store-manager");

                propertiesElement.ShouldContainPath("store-manager.$ref").ShouldBeSchemaReferenceId("to-one-staff-member-in-response");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.ShouldContainPath("backup-store-manager.$ref")
                    .ShouldBeSchemaReferenceId("nullable-to-one-staff-member-in-response").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeSchemaReferenceId("to-many-staff-member-in-response");
            });

            string? linksInRelationshipObjectSchemaRefId = null;
            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInRelationshipObjectSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("links-in-relationship-object").SchemaReferenceId;

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.oneOf[0].$ref")
                    .ShouldBeSchemaReferenceId("staff-member-identifier").SchemaReferenceId;

                propertiesElement.ShouldContainPath("data.oneOf[1].$ref").ShouldBeSchemaReferenceId("null-value");
            });

            schemasElement.ShouldContainPath($"{linksInRelationshipObjectSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("self");
                propertiesElement.Should().ContainProperty("related");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeSchemaReferenceId("staff-member-resource-type")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeSchemaReferenceId("staff-members");
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
                operationElement.ShouldBeString("get-supermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("supermarket-primary-response-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref").ShouldBeSchemaReferenceId("links-in-resource-document")
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

        document.ShouldContainPath("paths./supermarkets/{id}/store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-store-manager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staff-member-secondary-response-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeSchemaReferenceId("staff-member-data-in-response")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeSchemaReferenceId("staff-member-attributes-in-response");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/backup-store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-backup-store-manager");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("nullable-staff-member-secondary-response-document");
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
                operationElement.ShouldBeString("get-supermarket-cashiers");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
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

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-store-manager-relationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staff-member-identifier-response-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("links-in-resource-identifier-document").SchemaReferenceId;
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
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/backup-store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-backup-store-manager-relationship");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
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

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-cashiers-relationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("staff-member-identifier-collection-response-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? linksInResourceIdentifierCollectionDocumentSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                linksInResourceIdentifierCollectionDocumentSchemaRefId = propertiesElement.ShouldContainPath("links.$ref")
                    .ShouldBeSchemaReferenceId("links-in-resource-identifier-collection-document").SchemaReferenceId;
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
                operationElement.ShouldBeString("post-supermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("supermarket-post-request-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeSchemaReferenceId("supermarket-data-in-post-request")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeSchemaReferenceId("supermarket-attributes-in-post-request");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeSchemaReferenceId("supermarket-relationships-in-post-request").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("store-manager");
                propertiesElement.ShouldContainPath("store-manager.$ref").ShouldBeSchemaReferenceId("to-one-staff-member-in-request");

                propertiesElement.Should().ContainProperty("backup-store-manager");
                propertiesElement.ShouldContainPath("backup-store-manager.$ref").ShouldBeSchemaReferenceId("nullable-to-one-staff-member-in-request");

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeSchemaReferenceId("to-many-staff-member-in-request");
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
                operationElement.ShouldBeString("post-supermarket-cashiers-relationship");
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
                operationElement.ShouldBeString("patch-supermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeSchemaReferenceId("supermarket-patch-request-document").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeSchemaReferenceId("supermarket-data-in-patch-request")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeSchemaReferenceId("supermarket-attributes-in-patch-request");
                propertiesElement.ShouldContainPath("relationships.$ref").ShouldBeSchemaReferenceId("supermarket-relationships-in-patch-request");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/store-manager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patch-supermarket-store-manager-relationship");
            });
        });
    }

    [Fact]
    public async Task Casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/backup-store-manager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patch-supermarket-backup-store-manager-relationship");
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
                operationElement.ShouldBeString("patch-supermarket-cashiers-relationship");
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
                operationElement.ShouldBeString("delete-supermarket");
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
                operationElement.ShouldBeString("delete-supermarket-cashiers-relationship");
            });
        });
    }
}
