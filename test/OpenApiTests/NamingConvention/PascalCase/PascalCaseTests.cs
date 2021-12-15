using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConvention.PascalCase;

public sealed class PascalCaseTests : IClassFixture<OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
{
    private readonly OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

    public PascalCaseTests(OpenApiTestContext<PascalCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
    {
        _testContext = testContext;
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketCollection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("SupermarketCollectionResponseDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("LinksInResourceCollectionDocument");
                propertiesElement.ShouldContainPath("jsonapi.$ref").ShouldBeReferenceSchemaId("JsonapiObject");

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeReferenceSchemaId("SupermarketDataInResponse")
                    .SchemaReferenceId;
            });

            string? resourceAttributesInResponseSchemaRefId = null;
            string? resourceRelationshipInResponseSchemaRefId = null;
            string? primaryResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("SupermarketResourceType")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.$ref")
                    .ShouldBeReferenceSchemaId("SupermarketAttributesInResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("SupermarketRelationshipsInResponse").SchemaReferenceId;

                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("LinksInResourceObject");
            });

            schemasElement.ShouldContainPath($"{primaryResourceTypeSchemaRefId}.enum[0]").With(enumValueElement =>
            {
                enumValueElement.ShouldBeString("Supermarkets");
            });

            schemasElement.ShouldContainPath($"{resourceAttributesInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("NameOfCity");
                propertiesElement.Should().ContainProperty("Kind");
                propertiesElement.ShouldContainPath("Kind.$ref").ShouldBeReferenceSchemaId("SupermarketType");
            });

            string? nullableToOneResourceResponseDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceRelationshipInResponseSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");

                propertiesElement.ShouldContainPath("StoreManager.$ref").ShouldBeReferenceSchemaId("ToOneStaffMemberInResponse");

                nullableToOneResourceResponseDataSchemaRefId = propertiesElement.ShouldContainPath("BackupStoreManager.$ref")
                    .ShouldBeReferenceSchemaId("NullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.ShouldContainPath("Cashiers.$ref").ShouldBeReferenceSchemaId("ToManyStaffMemberInResponse");
            });

            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("LinksInRelationshipObject");

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.oneOf[0].$ref")
                    .ShouldBeReferenceSchemaId("StaffMemberIdentifier").SchemaReferenceId;

                propertiesElement.ShouldContainPath("data.oneOf[1].$ref").ShouldBeReferenceSchemaId("NullValue");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("StaffMemberResourceType")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeReferenceSchemaId("StaffMembers");
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("SupermarketPrimaryResponseDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("LinksInResourceDocument");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetSecondary_endpoint_with_single_resource()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/StoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("StaffMemberSecondaryResponseDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("StaffMemberDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("StaffMemberAttributesInResponse");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/BackupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketBackupStoreManager");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("NullableStaffMemberSecondaryResponseDocument");
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/Cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketCashiers");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("StaffMemberCollectionResponseDocument");
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/StoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("StaffMemberIdentifierResponseDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("LinksInResourceIdentifierDocument");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketBackupStoreManagerRelationship");
            });

            getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("NullableStaffMemberIdentifierResponseDocument");
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_GetRelationship_endpoint_with_ToMany_relationship()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("GetSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("StaffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("LinksInResourceIdentifierCollectionDocument");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_Post_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PostSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("SupermarketPostRequestDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("SupermarketDataInPostRequest")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("SupermarketAttributesInPostRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("SupermarketRelationshipsInPostRequest").SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceRelationshipInPostRequestSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.Should().ContainProperty("StoreManager");
                propertiesElement.ShouldContainPath("StoreManager.$ref").ShouldBeReferenceSchemaId("ToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("BackupStoreManager");
                propertiesElement.ShouldContainPath("BackupStoreManager.$ref").ShouldBeReferenceSchemaId("NullableToOneStaffMemberInRequest");

                propertiesElement.Should().ContainProperty("Cashiers");
                propertiesElement.ShouldContainPath("Cashiers.$ref").ShouldBeReferenceSchemaId("ToManyStaffMemberInRequest");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PostSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_Patch_endpoint()
    {
        // Assert
        string? documentSchemaRefId = null;

        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("SupermarketPatchRequestDocument").SchemaReferenceId;
        });

        _testContext.Document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("SupermarketDataInPatchRequest")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("SupermarketAttributesInPatchRequest");
                propertiesElement.ShouldContainPath("relationships.$ref").ShouldBeReferenceSchemaId("SupermarketRelationshipsInPatchRequest");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/StoreManager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarketStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/BackupStoreManager.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarketBackupStoreManagerRelationship");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("PatchSupermarketCashiersRelationship");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_Delete_endpoint()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("DeleteSupermarket");
            });
        });
    }

    [Fact]
    public void Pascal_casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Assert
        _testContext.Document.ShouldContainPath("paths./Supermarkets/{id}/relationships/Cashiers.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("DeleteSupermarketCashiersRelationship");
            });
        });
    }
}
