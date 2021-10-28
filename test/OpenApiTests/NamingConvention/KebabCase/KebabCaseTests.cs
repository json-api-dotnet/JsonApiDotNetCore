using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.NamingConvention.KebabCase
{
    public sealed class KebabCaseTests
        : IClassFixture<IntegrationTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext>>
    {
        private static Lazy<Task<JsonDocument>> _lazyOpenApiDocument;
        private readonly IntegrationTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> _testContext;

        public KebabCaseTests(IntegrationTestContext<KebabCaseNamingConventionStartup<NamingConventionDbContext>, NamingConventionDbContext> testContext)
        {
            _testContext = testContext;

            _lazyOpenApiDocument ??= new Lazy<Task<JsonDocument>>(async () =>
            {
                testContext.UseController<SupermarketsController>();

                string content = await GetAsync("swagger/v1/swagger.json");

                await WriteToSwaggerDocumentsFolderAsync(content);

                return JsonDocument.Parse(content);
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private async Task<string> GetAsync(string requestUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            using HttpClient client = _testContext.Factory.CreateClient();
            HttpResponseMessage responseMessage = await client.SendAsync(request);

            return await responseMessage.Content.ReadAsStringAsync();
        }

        private static async Task WriteToSwaggerDocumentsFolderAsync(string content)
        {
            string path = GetSwaggerDocumentPath(nameof(KebabCase));
            await File.WriteAllTextAsync(path, content);
        }

        private static string GetSwaggerDocumentPath(string fileName)
        {
            string swaggerDocumentsDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            return Path.Join(swaggerDocumentsDirectory, "SwaggerDocuments", $"{fileName}.json");
        }

        [Fact]
        public async Task Kebab_naming_policy_is_applied_to_get_collection_endpoint()
        {
            // Arrange
            const string expectedReferenceIdForResponseDocument = "supermarket-collection-response-document";
            const string expectedReferenceIdForToOneResourceResponseData = "to-one-staff-member-response-data";
            const string expectedReferenceIdForToManyResourceResponseData = "to-many-staff-member-response-data";
            const string expectedReferenceIdForResourceDataInResponse = "supermarket-data-in-response";
            const string expectedReferenceIdForResourceIdentifier = "staff-member-identifier";
            const string expectedReferenceIdResourceAttributesInResponse = "supermarket-attributes-in-response";
            const string expectedReferenceIdResourceRelationshipsInResponse = "supermarket-relationships-in-response";
            const string expectedReferenceIdForLinksInRelationshipObject = "links-in-relationship-object";
            const string expectedReferenceIdForLinksResourceObject = "links-in-resource-object";
            const string expectedReferenceIdForTopLevelLinks = "links-in-resource-collection-document";
            const string expectedReferenceIdForJsonapiObject = "jsonapi-object";
            const string expectedReferenceIdForNullValue = "null-value";
            const string expectedReferenceIdForEnum = "supermarket-type";

            const string expectedOperationId = "get-supermarket-collection";
            const string expectedPrimaryResourcePublicName = "supermarkets";
            string expectedPrimaryResourceType = $"{expectedPrimaryResourcePublicName}-resource-type";
            string expectedPath = $"/{expectedPrimaryResourcePublicName}";
            const string expectedPropertyNameForStringAttribute = "name-of-city";
            const string expectedPropertyNameForEnumAttribute = "kind";
            const string expectedPropertyNameForToOneRelationship = "store-manager";
            const string expectedPropertyNameForToManyRelationship = "cashiers";
            const string expectedRelatedResourceType = "staff-members-resource-type";
            const string expectedRelatedResourcePublicName = "staff-members";

            // Act
            JsonDocument document = await _lazyOpenApiDocument.Value;

            // Assert
            document.SelectTokenOrError("paths").Should().HaveProperty(expectedPath);
            document.SelectTokenOrError($"paths.{expectedPath}.get.operationId").GetString().Should().Be(expectedOperationId);

            string responseRefId = document.SelectTokenOrError($"paths.{expectedPath}.get.responses.200.content['application/vnd.api+json'].schema.$ref")
                .GetReferenceSchemaId();

            responseRefId.Should().Be(expectedReferenceIdForResponseDocument);

            string topLevelLinksRefId = document.SelectTokenOrError($"components.schemas.{responseRefId}.properties.links.$ref").GetReferenceSchemaId();
            topLevelLinksRefId.Should().Be(expectedReferenceIdForTopLevelLinks);

            string jsonapiRefId = document.SelectTokenOrError($"components.schemas.{responseRefId}.properties.jsonapi.$ref").GetReferenceSchemaId();
            jsonapiRefId.Should().Be(expectedReferenceIdForJsonapiObject);

            string dataRefId = document.SelectTokenOrError($"components.schemas.{responseRefId}.properties.data.items.$ref").GetReferenceSchemaId();
            dataRefId.Should().Be(expectedReferenceIdForResourceDataInResponse);

            string resourceLinksRefId = document.SelectTokenOrError($"components.schemas.{dataRefId}.properties.links.$ref").GetReferenceSchemaId();
            resourceLinksRefId.Should().Be(expectedReferenceIdForLinksResourceObject);

            string primaryResourceTypeRefId = document.SelectTokenOrError($"components.schemas.{dataRefId}.properties.type.$ref").GetReferenceSchemaId();
            primaryResourceTypeRefId.Should().Be(expectedPrimaryResourceType);
            string primaryResourceTypeValue = document.SelectTokenOrError($"components.schemas.{primaryResourceTypeRefId}.enum[0]").GetString();
            primaryResourceTypeValue.Should().Be(expectedPrimaryResourcePublicName);

            string attributesRefId = document.SelectTokenOrError($"components.schemas.{dataRefId}.properties.attributes.$ref").GetReferenceSchemaId();
            attributesRefId.Should().Be(expectedReferenceIdResourceAttributesInResponse);
            JsonElement attributes = document.SelectTokenOrError($"components.schemas.{attributesRefId}.properties");

            attributes.Should().HaveProperty(expectedPropertyNameForStringAttribute);
            attributes.Should().HaveProperty(expectedPropertyNameForEnumAttribute);
            string enumRefId = document.SelectTokenOrError($"components.schemas.{attributesRefId}.properties.kind.$ref").GetReferenceSchemaId();
            enumRefId.Should().Be(expectedReferenceIdForEnum);

            string relationshipsRefId = document.SelectTokenOrError($"components.schemas.{dataRefId}.properties.relationships.$ref").GetReferenceSchemaId();
            relationshipsRefId.Should().Be(expectedReferenceIdResourceRelationshipsInResponse);

            JsonElement relationships = document.SelectTokenOrError($"components.schemas.{relationshipsRefId}.properties");
            relationships.Should().HaveProperty(expectedPropertyNameForToOneRelationship);

            string toOneResourceRefId = document.SelectTokenOrError($"components.schemas.{relationshipsRefId}.properties.store-manager.$ref")
                .GetReferenceSchemaId();

            toOneResourceRefId.Should().Be(expectedReferenceIdForToOneResourceResponseData);
            relationships.Should().HaveProperty(expectedPropertyNameForToManyRelationship);
            string toManyResourceRefId = document.SelectTokenOrError($"components.schemas.{relationshipsRefId}.properties.cashiers.$ref").GetReferenceSchemaId();
            toManyResourceRefId.Should().Be(expectedReferenceIdForToManyResourceResponseData);

            string relationshipLinksRefId = document.SelectTokenOrError($"components.schemas.{toOneResourceRefId}.properties.links.$ref").GetReferenceSchemaId();
            relationshipLinksRefId.Should().Be(expectedReferenceIdForLinksInRelationshipObject);

            string resourceIdentifierRefId = document.SelectTokenOrError($"components.schemas.{toOneResourceRefId}.properties.data.oneOf[0].$ref")
                .GetReferenceSchemaId();

            resourceIdentifierRefId.Should().Be(expectedReferenceIdForResourceIdentifier);
            string nullValueRefId = document.SelectTokenOrError($"components.schemas.{toOneResourceRefId}.properties.data.oneOf[1].$ref").GetReferenceSchemaId();
            nullValueRefId.Should().Be(expectedReferenceIdForNullValue);

            string relatedResourceTypeReferenceSchema =
                document.SelectTokenOrError($"components.schemas.{resourceIdentifierRefId}.properties.type.$ref").GetReferenceSchemaId();

            relatedResourceTypeReferenceSchema.Should().Be(expectedRelatedResourceType);

            string relatedResourceTypeValue =
                document.SelectTokenOrError($"components.schemas.{relatedResourceTypeReferenceSchema}.enum[0]").GetReferenceSchemaId();

            relatedResourceTypeValue.Should().Be(expectedRelatedResourcePublicName);
        }

        [Fact]
        public async Task Kebab_naming_policy_is_applied_to_get_single_endpoint()
        {
            // Arrange
            const string expectedReferenceIdForResponseDocument = "supermarket-primary-response-document";
            const string expectedReferenceIdForTopLevelLinks = "links-in-resource-document";

            const string expectedOperationId = "get-supermarket";
            const string expectedPrimaryResourcePublicName = "supermarkets";
            string expectedPath = $"/{expectedPrimaryResourcePublicName}/{{id}}";

            // Act
            JsonDocument document = await _lazyOpenApiDocument.Value;

            // Assert
            document.SelectTokenOrError("paths").Should().HaveProperty(expectedPath);
            document.SelectTokenOrError($"paths.{expectedPath}.get.tags[0]").GetString().Should().Be(expectedPrimaryResourcePublicName);
            document.SelectTokenOrError($"paths.{expectedPath}.get.operationId").GetString().Should().Be(expectedOperationId);

            string responseRefId = document.SelectTokenOrError($"paths.{expectedPath}.get.responses.200.content['application/vnd.api+json'].schema.$ref")
                .GetReferenceSchemaId();

            responseRefId.Should().Be(expectedReferenceIdForResponseDocument);

            string topLevelLinksRefId = document.SelectTokenOrError($"components.schemas.{responseRefId}.properties.links.$ref").GetReferenceSchemaId();
            topLevelLinksRefId.Should().Be(expectedReferenceIdForTopLevelLinks);
        }
    }
}
