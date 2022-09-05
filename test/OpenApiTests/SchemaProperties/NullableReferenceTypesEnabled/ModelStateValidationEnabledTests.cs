using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

public sealed class ModelStateValidationEnabledTests
    : IClassFixture<OpenApiTestContext<SchemaPropertiesStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<SchemaPropertiesStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> _testContext;

    public ModelStateValidationEnabledTests(
        OpenApiTestContext<SchemaPropertiesStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CowsController>();
    }

    [Fact]
    public async Task Resource_when_ModelStateValidation_is_enabled_produces_expected_required_property_in_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInPostRequest.required").With(requiredElement =>
        {
            var requiredAttributes = JsonSerializer.Deserialize<List<string>>(requiredElement.GetRawText());
            requiredAttributes.ShouldNotBeNull();

            requiredAttributes.Should().Contain("name");
            requiredAttributes.Should().Contain("nameOfCurrentFarm");
            requiredAttributes.Should().Contain("nickname");
            requiredAttributes.Should().Contain("weight");
            requiredAttributes.Should().Contain("hasProducedMilk");

            requiredAttributes.ShouldHaveCount(5);
        });
    }
}
