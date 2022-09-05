using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<SchemaPropertiesStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<SchemaPropertiesStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> _testContext;

    public NullabilityTests(
        OpenApiTestContext<SchemaPropertiesStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ChickensController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/SchemaProperties/NullableReferenceTypesDisabled";
    }

    [Fact]
    public async Task Resource_produces_expected_nullable_properties_in_schema()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInResponse.properties").With(propertiesElement =>
        {
            propertiesElement.ShouldContainPath("name").With(propertyElement =>
            {
                propertyElement.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });

            propertiesElement.ShouldContainPath("nameOfCurrentFarm").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });

            propertiesElement.ShouldContainPath("age").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });

            propertiesElement.ShouldContainPath("weight").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });

            propertiesElement.ShouldContainPath("timeAtCurrentFarmInDays").With(propertyElement =>
            {
                propertyElement.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });

            propertiesElement.ShouldContainPath("hasProducedEggs").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });
        });
    }
}
