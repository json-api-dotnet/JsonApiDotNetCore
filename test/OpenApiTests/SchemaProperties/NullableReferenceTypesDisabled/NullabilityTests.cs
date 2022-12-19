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
        document.ShouldContainPath("components.schemas.chickenAttributesInResponse.properties").With(propertiesElement =>
        {
            propertiesElement.ShouldContainPath("name").With(propertyElement =>
            {
                propertyElement.ShouldContainPath("nullable").With(element => element.ValueKind.Should().Be(JsonValueKind.True));
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
                propertyElement.ShouldContainPath("nullable").With(element => element.ValueKind.Should().Be(JsonValueKind.True));
            });

            propertiesElement.ShouldContainPath("hasProducedEggs").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });
        });
    }
}
