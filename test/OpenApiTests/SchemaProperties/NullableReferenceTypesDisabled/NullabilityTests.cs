using System.Text.Json;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.SchemaProperties.NullableReferenceTypesDisabled;

public sealed class NullabilityTests
    : IClassFixture<OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> _testContext;

    public NullabilityTests(OpenApiTestContext<OpenApiStartup<NullableReferenceTypesDisabledDbContext>, NullableReferenceTypesDisabledDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ChickensController>();
        testContext.SwaggerDocumentOutputPath = "test/OpenApiClientTests/SchemaProperties/NullableReferenceTypesDisabled";
    }

    [Fact]
    public async Task Produces_expected_nullable_properties_in_schema_for_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.chickenAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath("name").With(schemaProperty =>
            {
                schemaProperty.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });

            schemaProperties.ShouldContainPath("nameOfCurrentFarm").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });

            schemaProperties.ShouldContainPath("age").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });

            schemaProperties.ShouldContainPath("weight").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });

            schemaProperties.ShouldContainPath("timeAtCurrentFarmInDays").With(schemaProperty =>
            {
                schemaProperty.ShouldContainPath("nullable").With(nullableProperty => nullableProperty.ValueKind.Should().Be(JsonValueKind.True));
            });

            schemaProperties.ShouldContainPath("hasProducedEggs").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });
        });
    }
}
