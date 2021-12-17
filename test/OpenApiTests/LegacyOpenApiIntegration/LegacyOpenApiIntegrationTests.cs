using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.LegacyOpenApiIntegration;

public sealed class LegacyOpenApiIntegrationTests : OpenApiTestContext<LegacyOpenApiIntegrationStartup<LegacyIntegrationDbContext>, LegacyIntegrationDbContext>
{
    public LegacyOpenApiIntegrationTests()
    {
        UseController<AirplanesController>();
        UseController<FlightsController>();
        UseController<FlightAttendantsController>();

        SwaggerDocumentOutputPath = "test/OpenApiClientTests/LegacyClient";
    }

    [Fact]
    public async Task Retrieved_swagger_document_matches_expected_document()
    {
        // Act
        JsonElement jsonElement = await GetSwaggerDocumentAsync();

        // Assert
        string expectedJsonText = await GetExpectedSwaggerDocumentAsync();
        string actualJsonText = jsonElement.ToString();
        actualJsonText.Should().BeJson(expectedJsonText);
    }

    private static async Task<string> GetExpectedSwaggerDocumentAsync()
    {
        const string embeddedResourceName = $"{nameof(OpenApiTests)}.{nameof(LegacyOpenApiIntegration)}.swagger.json";
        var assembly = Assembly.GetExecutingAssembly();

        await using Stream? stream = assembly.GetManifestResourceStream(embeddedResourceName);

        if (stream == null)
        {
            throw new Exception($"Failed to load embedded resource '{embeddedResourceName}'. Set Build Action to Embedded Resource in properties.");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
