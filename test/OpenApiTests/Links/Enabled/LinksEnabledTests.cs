using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Resources.Annotations;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.Links.Enabled;

public sealed class LinksEnabledTests : IClassFixture<OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> _testContext;

    public LinksEnabledTests(OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<VacationsController>();
        testContext.UseController<AccommodationsController>();
        testContext.UseController<TransportsController>();
        testContext.UseController<ExcursionsController>();

        testContext.SwaggerDocumentOutputDirectory = $"{GetType().Namespace!.Replace('.', '/')}/GeneratedSwagger";
    }

    [Theory]
    [InlineData("resourceTopLevelLinks", LinkTypes.Self | LinkTypes.DescribedBy)]
    [InlineData("resourceCollectionTopLevelLinks", LinkTypes.Self | LinkTypes.DescribedBy | LinkTypes.Pagination)]
    [InlineData("resourceIdentifierTopLevelLinks", LinkTypes.Self | LinkTypes.Related | LinkTypes.DescribedBy)]
    [InlineData("resourceIdentifierCollectionTopLevelLinks", LinkTypes.Self | LinkTypes.Related | LinkTypes.DescribedBy | LinkTypes.Pagination)]
    [InlineData("errorTopLevelLinks", LinkTypes.Self | LinkTypes.DescribedBy)]
    [InlineData("relationshipLinks", LinkTypes.Self | LinkTypes.Related)]
    [InlineData("resourceLinks", LinkTypes.Self)]
    public async Task All_configurable_link_schemas_are_exposed(string schemaId, LinkTypes expected)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath(schemaId).With(linksElement =>
            {
                linksElement.Should().NotContainPath("required");

                linksElement.Should().ContainPath("properties").With(propertiesElement =>
                {
                    string[] propertyNamesExpected = expected.ToPropertyNames().ToArray();
                    string[] linkPropertyNames = propertiesElement.EnumerateObject().Select(propertyElement => propertyElement.Name).ToArray();

                    linkPropertyNames.Should().BeEquivalentTo(propertyNamesExpected);
                });
            });
        });
    }

    [Theory]
    [InlineData("resourceTopLevelLinks", "accommodationPrimaryResponseDocument", true)]
    [InlineData("resourceTopLevelLinks", "accommodationSecondaryResponseDocument", true)]
    [InlineData("resourceTopLevelLinks", "excursionPrimaryResponseDocument", true)]
    [InlineData("resourceTopLevelLinks", "transportPrimaryResponseDocument", true)]
    [InlineData("resourceTopLevelLinks", "nullableTransportSecondaryResponseDocument", true)]
    [InlineData("resourceTopLevelLinks", "vacationPrimaryResponseDocument", true)]
    [InlineData("resourceCollectionTopLevelLinks", "accommodationCollectionResponseDocument", true)]
    [InlineData("resourceCollectionTopLevelLinks", "excursionCollectionResponseDocument", true)]
    [InlineData("resourceCollectionTopLevelLinks", "transportCollectionResponseDocument", true)]
    [InlineData("resourceCollectionTopLevelLinks", "vacationCollectionResponseDocument", true)]
    [InlineData("resourceIdentifierTopLevelLinks", "accommodationIdentifierResponseDocument", true)]
    [InlineData("resourceIdentifierTopLevelLinks", "nullableTransportIdentifierResponseDocument", true)]
    [InlineData("resourceIdentifierCollectionTopLevelLinks", "excursionIdentifierCollectionResponseDocument", true)]
    [InlineData("errorTopLevelLinks", "errorResponseDocument", true)]
    [InlineData("relationshipLinks", "toOneAccommodationInResponse", false)]
    [InlineData("relationshipLinks", "toManyExcursionInResponse", false)]
    [InlineData("relationshipLinks", "nullableToOneTransportInResponse", false)]
    [InlineData("resourceLinks", "accommodationDataInResponse.allOf[1]", false)]
    [InlineData("resourceLinks", "excursionDataInResponse.allOf[1]", false)]
    [InlineData("resourceLinks", "transportDataInResponse.allOf[1]", false)]
    [InlineData("resourceLinks", "vacationDataInResponse.allOf[1]", false)]
    public async Task All_container_schemas_contain_correct_link_property(string linkSchemaId, string containerSchemaId, bool isRequired)
    {
        // Act
        JsonElement document = await _testContext.GetSwaggerDocumentAsync();

        // Assert
        document.Should().ContainPath("components.schemas").With(schemasElement =>
        {
            schemasElement.Should().ContainPath(containerSchemaId).With(containerElement =>
            {
                if (isRequired)
                {
                    containerElement.Should().ContainPath("required").With(requiredElement =>
                    {
                        requiredElement.Should().ContainArrayElement("links");
                    });
                }
                else
                {
                    if (containerElement.TryGetProperty("required", out JsonElement requiredElement))
                    {
                        requiredElement.Should().NotContainArrayElement("links");
                    }
                    else
                    {
                        containerElement.Should().NotContainPath("required");
                    }
                }

                containerElement.Should().ContainPath("properties.links.allOf[0].$ref").ShouldBeSchemaReferenceId(linkSchemaId);
            });
        });
    }
}
