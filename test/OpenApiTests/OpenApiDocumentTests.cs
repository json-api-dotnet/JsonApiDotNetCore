using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests
{
    public sealed class OpenApiDocumentTests : IntegrationTestContext<OpenApiStartup<OpenApiDbContext>, OpenApiDbContext>
    {
        public OpenApiDocumentTests()
        {
            UseController<AirplanesController>();
            UseController<FlightsController>();
        }

        [Fact]
        public async Task Retrieved_document_should_match_expected_document()
        {
            // Arrange
            string embeddedResourceName = $"{nameof(OpenApiTests)}.openapi.json";
            string expectedDocument = await LoadEmbeddedResourceAsync(embeddedResourceName);
            string requestUrl = $"swagger/{nameof(OpenApiTests)}/swagger.json";

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
