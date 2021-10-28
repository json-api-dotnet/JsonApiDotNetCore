using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class LegacyOpenApiIntegrationTests
        : IntegrationTestContext<LegacyOpenApiIntegrationStartup<LegacyIntegrationDbContext>, LegacyIntegrationDbContext>
    {
        public LegacyOpenApiIntegrationTests()
        {
            UseController<AirplanesController>();
            UseController<FlightsController>();
            UseController<FlightAttendantsController>();
        }

        // TODO: This test fails when all tests are openapi tests run in parallel; something isn't going right with the fixtures.
        [Fact]
        public async Task Retrieved_document_matches_expected_document()
        {
            // Arrange
            string embeddedResourceName = $"{nameof(OpenApiTests)}.SwaggerDocuments.{nameof(LegacyOpenApiIntegration)}.json";
            string expectedDocument = await LoadEmbeddedResourceAsync(embeddedResourceName);
            const string requestUrl = "swagger/v1/swagger.json";

            // Act
            string actualDocument = await GetAsync(requestUrl);

            // Assert
            actualDocument.Should().BeJson(expectedDocument);
        }

        private async Task<string> GetAsync(string requestUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            using HttpClient client = Factory.CreateClient();
            HttpResponseMessage responseMessage = await client.SendAsync(request);

            return await responseMessage.Content.ReadAsStringAsync();
        }

        private static async Task<string> LoadEmbeddedResourceAsync(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            await using Stream stream = assembly.GetManifestResourceStream(name);

            if (stream == null)
            {
                throw new Exception($"Failed to load embedded resource '{name}'. Set Build Action to Embedded Resource in properties.");
            }

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
