using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConvention.KebabCase;

public sealed class KebabCaseTests : IClassFixture<OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
{
    private readonly OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

    public KebabCaseTests(OpenApiTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-collection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarket-collection-response-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("links-in-resource-collection-document");
                propertiesElement.ShouldContainPath("jsonapi.$ref").ShouldBeReferenceSchemaId("jsonapi-object");

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeReferenceSchemaId("supermarket-data-in-response")
                    .SchemaReferenceId;
            });

            string? resourceAttributesInResponseSchemaRefId = null;
            string? resourceRelationshipInResponseSchemaRefId = null;
            string? primaryResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("supermarket-resource-type")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.$ref")
                    .ShouldBeReferenceSchemaId("supermarket-attributes-in-response").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("supermarket-relationships-in-response").SchemaReferenceId;

                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("links-in-resource-object");
            });

            schemasElement.ShouldContainPath($"{primaryResourceTypeSchemaRefId}.enum[0]").With(enumValueElement =>
            {
                enumValueElement.ShouldBeString("supermarkets");
            });

            schemasElement.ShouldContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("name-of-city");
                propertiesElement.Should().ContainProperty("kind");
                propertiesElement.ShouldContainPath("kind.$ref").ShouldBeReferenceSchemaId("supermarket-type");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("store-manager");

                propertiesElement.ShouldContainPath("store-manager.$ref").ShouldBeReferenceSchemaId("to-one-staff-member-in-response");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.ShouldContainPath("backup-store-manager.$ref")
                    .ShouldBeReferenceSchemaId("nullable-to-one-staff-member-in-response").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeReferenceSchemaId("to-many-staff-member-in-response");
            });

            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("links-in-relationship-object");

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.oneOf[0].$ref")
                    .ShouldBeReferenceSchemaId("staff-member-identifier").SchemaReferenceId;

                propertiesElement.ShouldContainPath("data.oneOf[1].$ref").ShouldBeReferenceSchemaId("null-value");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("staff-member-resource-type")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeReferenceSchemaId("staff-members");
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarket-primary-response-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("links-in-resource-document");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetSecondary_endpoint_with_single_resource()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-store-manager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staff-member-secondary-response-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("staff-member-data-in-response")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("staff-member-attributes-in-response");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/backup-store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-backup-store-manager");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("nullable-staff-member-secondary-response-document");
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-cashiers");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staff-member-collection-response-document");
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-store-manager-relationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staff-member-identifier-response-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("links-in-resource-identifier-document");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/backup-store-manager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-backup-store-manager-relationship");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("nullable-staff-member-identifier-response-document");
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_GetRelationship_endpoint_with_ToMany_relationship()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("get-supermarket-cashiers-relationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staff-member-identifier-collection-response-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("links-in-resource-identifier-collection-document");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_Post_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("post-supermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarket-post-request-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("supermarket-data-in-post-request")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("supermarket-attributes-in-post-request");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("supermarket-relationships-in-post-request").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("store-manager");
                propertiesElement.ShouldContainPath("store-manager.$ref").ShouldBeReferenceSchemaId("to-one-staff-member-in-request");

                propertiesElement.Should().ContainProperty("backup-store-manager");
                propertiesElement.ShouldContainPath("backup-store-manager.$ref").ShouldBeReferenceSchemaId("nullable-to-one-staff-member-in-request");

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeReferenceSchemaId("to-many-staff-member-in-request");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("post-supermarket-cashiers-relationship");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_Patch_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patch-supermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarket-patch-request-document").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("supermarket-data-in-patch-request")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("supermarket-attributes-in-patch-request");
                propertiesElement.ShouldContainPath("relationships.$ref").ShouldBeReferenceSchemaId("supermarket-relationships-in-patch-request");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/store-manager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patch-supermarket-store-manager-relationship");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/backup-store-manager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patch-supermarket-backup-store-manager-relationship");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patch-supermarket-cashiers-relationship");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_Delete_endpoint()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("delete-supermarket");
            });
        });
    }

    [Fact]
    public void Kebab_casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("delete-supermarket-cashiers-relationship");
            });
        });
    }
}
