using System.Reflection;
using System.Text.Json;
using Xunit;

namespace OpenApiTests.LegacyOpenApiIntegration;

public sealed class LegacyOpenApiIntegrationTests : OpenApiTestContext<LegacyOpenApiIntegrationStartup<LegacyIntegrationDbContext>, LegacyIntegrationDbContext>
{
    public LegacyOpenApiIntegrationTests()
    {
        UseController<AirplanesController>();
        UseController<FlightsController>();
        UseController<FlightAttendantsController>();
    }

    [Fact]
    public async Task Retrieved_document_matches_expected_document()
    {
        // Arrange
        const string embeddedResourceName = $"{nameof(OpenApiTests)}.{nameof(LegacyOpenApiIntegration)}.swagger.json";
        string expectedDocument = await LoadEmbeddedResourceAsync(embeddedResourceName);

        // Act
        JsonElement actualDocument = await LazyDocument.Value;

        // Assert
        actualDocument.Should().BeJson(expectedDocument);
    }

    private static async Task<string> LoadEmbeddedResourceAsync(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using Stream? stream = assembly.GetManifestResourceStream(name);

        if (stream == null)
        {
            throw new Exception($"Failed to load embedded resource '{name}'. Set Build Action to Embedded Resource in properties.");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
