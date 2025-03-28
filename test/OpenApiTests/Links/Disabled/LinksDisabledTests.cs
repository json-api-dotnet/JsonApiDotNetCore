using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiTests.Links.Disabled;

public sealed class LinksDisabledTests : IClassFixture<OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> _testContext;

    public LinksDisabledTests(OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;

        testContext.UseController<VacationsController>();
        testContext.UseController<AccommodationsController>();
        testContext.UseController<TransportsController>();
        testContext.UseController<ExcursionsController>();

        testContext.SetTestOutputHelper(testOutputHelper);

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.TopLevelLinks = LinkTypes.None;
        options.ResourceLinks = LinkTypes.NotConfigured;
        options.RelationshipLinks = LinkTypes.None;
    }

    [Theory]
    [InlineData("resourceTopLevelLinks")]
    [InlineData("resourceCollectionTopLevelLinks")]
    [InlineData("resourceIdentifierTopLevelLinks")]
    [InlineData("resourceIdentifierCollectionTopLevelLinks")]
    [InlineData("errorTopLevelLinks")]
    [InlineData("relationshipLinks")]
    [InlineData("resourceLinks")]
    public async Task All_configurable_link_schemas_are_hidden(string schemaId)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().NotContainPath(schemaId);
        });
    }

    [Fact]
    public async Task Error_links_schema_is_visible()
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas.errorLinks").With(linksElement =>
        {
            linksElement.Should().NotContainPath("required");

            linksElement.Should().ContainPath("properties").With(propertiesElement =>
            {
                string[] linkPropertyNames = propertiesElement.EnumerateObject().Select(propertyElement => propertyElement.Name).ToArray();

                linkPropertyNames.Should().HaveCount(2);
                linkPropertyNames[0].Should().Be("about");
                linkPropertyNames[1].Should().Be("type");
            });
        });
    }

    [Theory]
    [InlineData("primaryAccommodationResponseDocument")]
    [InlineData("secondaryAccommodationResponseDocument")]
    [InlineData("primaryExcursionResponseDocument")]
    [InlineData("primaryTransportResponseDocument")]
    [InlineData("nullableSecondaryTransportResponseDocument")]
    [InlineData("primaryVacationResponseDocument")]
    [InlineData("accommodationCollectionResponseDocument")]
    [InlineData("excursionCollectionResponseDocument")]
    [InlineData("transportCollectionResponseDocument")]
    [InlineData("vacationCollectionResponseDocument")]
    [InlineData("accommodationIdentifierResponseDocument")]
    [InlineData("nullableTransportIdentifierResponseDocument")]
    [InlineData("excursionIdentifierCollectionResponseDocument")]
    [InlineData("errorResponseDocument")]
    [InlineData("toOneAccommodationInResponse")]
    [InlineData("toManyExcursionInResponse")]
    [InlineData("nullableToOneTransportInResponse")]
    [InlineData("dataInAccommodationResponse.allOf[1]")]
    [InlineData("dataInExcursionResponse.allOf[1]")]
    [InlineData("dataInTransportResponse.allOf[1]")]
    [InlineData("dataInVacationResponse.allOf[1]")]
    public async Task All_container_schemas_contain_no_link_property(string containerSchemaId)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath(containerSchemaId).With(containerElement =>
            {
                if (containerElement.TryGetProperty("required", out JsonElement requiredElement))
                {
                    requiredElement.Should().NotContainArrayElement("links");
                }
                else
                {
                    containerElement.Should().NotContainPath("required");
                }

                containerElement.Should().NotContainPath("properties.links");
            });
        });
    }
}
