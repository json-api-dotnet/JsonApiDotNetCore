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
        UseController<PassengersController>();

        SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
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

    private async Task<string> GetExpectedSwaggerDocumentAsync()
    {
        string embeddedResourceName = $"{GetType().Namespace!.Replace('/', '.')}.swagger.json";
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
