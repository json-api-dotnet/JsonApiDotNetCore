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

    [Fact]
    public async Task Produces_expected_nullable_properties_in_schema_for_resource()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.ShouldContainPath("components.schemas.cowAttributesInResponse.properties").With(schemaProperties =>
        {
            schemaProperties.ShouldContainPath("name").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });

            schemaProperties.ShouldContainPath("nameOfCurrentFarm").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });

            schemaProperties.ShouldContainPath("nameOfPreviousFarm").With(schemaProperty =>
            {
                schemaProperty.ShouldContainPath("nullable").With(element => element.ValueKind.Should().Be(JsonValueKind.True));
            });

            schemaProperties.ShouldContainPath("nickname").With(schemaProperty =>
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
                schemaProperty.ShouldContainPath("nullable").With(element => element.ValueKind.Should().Be(JsonValueKind.True));
            });

            schemaProperties.ShouldContainPath("hasProducedMilk").With(schemaProperty =>
            {
                schemaProperty.ShouldNotContainPath("nullable");
            });
        });
    }
}
