using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesEnabled;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<OpenApiStartup<NullableReferenceTypesEnabledDbContext>, NullableReferenceTypesEnabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CowsController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/SchemaProperties/NullableReferenceTypesEnabled";
    }

    [Theory]
    [InlineData("nameOfPreviousFarm")]
    [InlineData("timeAtCurrentFarmInDays")]
    public async Task Property_in_schema_for_resource_should_be_nullable(string propertyName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath(propertyName).With(schemaProperty =>
            {
                schemaProperty.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });
        });
    }

    [Theory]
    [InlineData("name")]
    [InlineData("nameOfCurrentFarm")]
    [InlineData("nickname")]
    [InlineData("age")]
    [InlineData("weight")]
    [InlineData("hasProducedMilk")]
    public async Task Property_in_schema_for_resource_should_not_be_nullable(string attributeName)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath(attributeName).With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });
        });
    }
}
