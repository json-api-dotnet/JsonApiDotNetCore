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
        document.ShouldContainPath("components.schemas.cowAttributesInResponse.properties").With(propertiesElement =>
        {
            propertiesElement.ShouldContainPath("name").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });

            propertiesElement.ShouldContainPath("nameOfCurrentFarm").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });

            propertiesElement.ShouldContainPath("nameOfPreviousFarm").With(propertyElement =>
            {
                propertyElement.ShouldContainPath("nullable").With(element => element.ValueKind.Should().Be(JsonValueKind.True));
            });

            propertiesElement.ShouldContainPath("nickname").With(propertyElement =>
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

            propertiesElement.ShouldContainPath("hasProducedMilk").With(propertyElement =>
            {
                propertyElement.ShouldNotContainPath("nullable");
            });
        });
    }
}

