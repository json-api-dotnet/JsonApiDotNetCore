using System.Text.Json;
using OpenApiTests.Controllers;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConvention.CamelCase;

public sealed class CamelCaseTests
    : IClassFixture<IntegrationTestContext<CamelCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
{
    private static Lazy<Task<JsonElement>>? _lazyOpenApiDocument;
    private readonly IntegrationTestContext<CamelCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

    public CamelCaseTests(IntegrationTestContext<CamelCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
    {
        _testContext = testContext;

        _lazyOpenApiDocument ??= new Lazy<Task<JsonElement>>(async () =>
        {
            testContext.UseController<SupermarketsController>();

            string content = await GetAsync("swagger/v1/swagger.json");

            await WriteSwaggerDocumentToFileAsync(content);

            JsonDocument document = JsonDocument.Parse(content);

            return document.ToJsonElement();
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    [Fact]
    public async Task Camel_casing_convention_is_applied_to_GetCollection_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCollection");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketCollectionResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInResourceCollectionDocument");
                propertiesElement.ShouldContainPath("jsonapi.$ref").ShouldBeReferenceSchemaId("jsonapiObject");

                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.items.$ref").ShouldBeReferenceSchemaId("supermarketDataInResponse")
                    .SchemaReferenceId;
            });

            string? resourceAttributesInResponseSchemaRefId = null;
            string? resourceRelationshipInResponseSchemaRefId = null;
            string? primaryResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                primaryResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("supermarketResourceType")
                    .SchemaReferenceId;

                resourceAttributesInResponseSchemaRefId = propertiesElement.ShouldContainPath("attributes.$ref")
                    .ShouldBeReferenceSchemaId("supermarketAttributesInResponse").SchemaReferenceId;

                resourceRelationshipInResponseSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("supermarketRelationshipsInResponse").SchemaReferenceId;

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
                    .ShouldBeReferenceSchemaId("nullableToOneStaffMemberInResponse").SchemaReferenceId;

                propertiesElement.Should().ContainProperty("cashiers");
                propertiesElement.ShouldContainPath("cashiers.$ref").ShouldBeReferenceSchemaId("toManyStaffMemberInResponse");
            });

            string? relatedResourceIdentifierSchemaRefId = null;

            schemasElement.ShouldContainPath($"{nullableToOneResourceResponseDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("links.$ref").ShouldBeReferenceSchemaId("linksInRelationshipObject");

                relatedResourceIdentifierSchemaRefId = propertiesElement.ShouldContainPath("data.oneOf[0].$ref")
                    .ShouldBeReferenceSchemaId("staffMemberIdentifier").SchemaReferenceId;

                propertiesElement.ShouldContainPath("data.oneOf[1].$ref").ShouldBeReferenceSchemaId("nullValue");
            });

            string? relatedResourceTypeSchemaRefId = null;

            schemasElement.ShouldContainPath($"{relatedResourceIdentifierSchemaRefId}.properties").With(propertiesElement =>
            {
                relatedResourceTypeSchemaRefId = propertiesElement.ShouldContainPath("type.$ref").ShouldBeReferenceSchemaId("staffMemberResourceType")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{relatedResourceTypeSchemaRefId}.enum[0]").ShouldBeReferenceSchemaId("staffMembers");
        });
    }

    [Fact]
    public async Task Camel_casing_convention_is_applied_to_GetSingle_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketPrimaryResponseDocument").SchemaReferenceId;
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
    public async Task Camel_casing_convention_is_applied_to_GetSecondary_endpoint_with_single_resource()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/storeManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketStoreManager");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberSecondaryResponseDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("staffMemberDataInResponse")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("staffMemberAttributesInResponse");
            });
        });
    }

    [Fact]
    public async Task Camel_casing_convention_is_applied_to_GetSecondary_endpoint_with_nullable_resource()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_GetSecondary_endpoint_with_resources()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_GetRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/storeManager.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketStoreManagerRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberIdentifierResponseDocument").SchemaReferenceId;
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
    public async Task Camel_casing_convention_is_applied_to_GetRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_GetRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.get").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("getSupermarketCashiersRelationship");
            });

            documentSchemaRefId = getElement.ShouldContainPath("responses.200.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("staffMemberIdentifierCollectionResponseDocument").SchemaReferenceId;
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
    public async Task Camel_casing_convention_is_applied_to_Post_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets.post").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("postSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketPostRequestDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("supermarketDataInPostRequest")
                    .SchemaReferenceId;
            });

            string? resourceRelationshipInPostRequestSchemaRefId = null;

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("supermarketAttributesInPostRequest");

                resourceRelationshipInPostRequestSchemaRefId = propertiesElement.ShouldContainPath("relationships.$ref")
                    .ShouldBeReferenceSchemaId("supermarketRelationshipsInPostRequest").SchemaReferenceId;
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
    public async Task Camel_casing_convention_is_applied_to_PostRelationship_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_Patch_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        string? documentSchemaRefId = null;

        document.ShouldContainPath("paths./supermarkets/{id}.patch").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("patchSupermarket");
            });

            documentSchemaRefId = getElement.ShouldContainPath("requestBody.content['application/vnd.api+json'].schema.$ref")
                .ShouldBeReferenceSchemaId("supermarketPatchRequestDocument").SchemaReferenceId;
        });

        document.ShouldContainPath("components.schemas").With(schemasElement =>
        {
            string? resourceDataSchemaRefId = null;

            schemasElement.ShouldContainPath($"{documentSchemaRefId}.properties").With(propertiesElement =>
            {
                resourceDataSchemaRefId = propertiesElement.ShouldContainPath("data.$ref").ShouldBeReferenceSchemaId("supermarketDataInPatchRequest")
                    .SchemaReferenceId;
            });

            schemasElement.ShouldContainPath($"{resourceDataSchemaRefId}.properties").With(propertiesElement =>
            {
                propertiesElement.ShouldContainPath("attributes.$ref").ShouldBeReferenceSchemaId("supermarketAttributesInPatchRequest");
                propertiesElement.ShouldContainPath("relationships.$ref").ShouldBeReferenceSchemaId("supermarketRelationshipsInPatchRequest");
            });
        });
    }

    [Fact]
    public async Task Camel_casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToOne_relationship()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_PatchRelationship_endpoint_with_nullable_ToOne_relationship()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_PatchRelationship_endpoint_with_ToMany_relationship()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_Delete_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

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
    public async Task Camel_casing_convention_is_applied_to_DeleteRelationship_endpoint()
    {
        // Act
        JsonElement document = await _lazyOpenApiDocument!.Value;

        // Assert
        document.ShouldContainPath("paths./supermarkets/{id}/relationships/cashiers.delete").With(getElement =>
        {
            getElement.ShouldContainPath("operationId").With(operationElement =>
            {
                operationElement.ShouldBeString("deleteSupermarketCashiersRelationship");
            });
        });
    }

    private async Task<string> GetAsync(string requestUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        using HttpClient client = _testContext.Factory.CreateClient();
        using HttpResponseMessage responseMessage = await client.SendAsync(request);

        return await responseMessage.Content.ReadAsStringAsync();
    }

    private async Task WriteSwaggerDocumentToFileAsync(string document)
    {
        string testSuitePath = GetTestSuitePath();
        string documentPath = Path.Join(testSuitePath, "swagger.json");
        await File.WriteAllTextAsync(documentPath, document);
    }

    private string GetTestSuitePath()
    {
        string solutionTestDirectoryPath = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.Parent!.FullName;
        string currentNamespacePathRelativeToTestDirectory = Path.Join(GetType().Namespace!.Split('.'));

        return Path.Join(solutionTestDirectoryPath, currentNamespacePathRelativeToTestDirectory);
    }
}
