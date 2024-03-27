using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests.Links.Mixed;

public sealed class LinksMixedTests : IClassFixture<OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext>>
{
    private readonly OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> _testContext;

    public LinksMixedTests(OpenApiTestContext<OpenApiStartup<LinkDbContext>, LinkDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<VacationsController>();
        testContext.UseController<AccommodationsController>();
        testContext.UseController<TransportsController>();
        testContext.UseController<ExcursionsController>();

        testContext.ConfigureServices(services => services.AddSingleton(CreateResourceGraph));

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.TopLevelLinks = LinkTypes.Pagination;
        options.ResourceLinks = LinkTypes.NotConfigured;
        options.RelationshipLinks = LinkTypes.None;
    }

    [Theory]
    [InlineData("resourceTopLevelLinks", LinkTypes.Self)]
    [InlineData("resourceCollectionTopLevelLinks", LinkTypes.Self)]
    [InlineData("resourceIdentifierTopLevelLinks", LinkTypes.Self | LinkTypes.Related)]
    [InlineData("resourceIdentifierCollectionTopLevelLinks", LinkTypes.Self | LinkTypes.Related)]
    [InlineData("errorTopLevelLinks", LinkTypes.Self)]
    [InlineData("relationshipLinks", LinkTypes.Self | LinkTypes.Related)]
    [InlineData("resourceLinks", LinkTypes.Self)]
    public async Task Expected_configurable_link_schemas_are_exposed(string schemaId, LinkTypes expected)
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

    private static IResourceGraph CreateResourceGraph(IServiceProvider serviceProvider)
    {
        ResourceGraphBuilder builder = CreateResourceGraphBuilder(serviceProvider);
        var editor = new ResourceGraphEditor(builder);

        editor.ChangeLinksInResourceType(typeof(Vacation), LinkTypes.None, null, null);
        editor.ChangeLinkInRelationship(typeof(Vacation), nameof(Vacation.Transport), LinkTypes.Related);

        editor.ChangeLinksInResourceType(typeof(Accommodation), LinkTypes.Self, LinkTypes.Self, null);

        editor.ChangeLinksInResourceType(typeof(Transport), LinkTypes.Related, LinkTypes.Self, LinkTypes.Self);

        editor.ChangeLinksInResourceType(typeof(Excursion), LinkTypes.None, null, LinkTypes.None);

        return editor.GetResourceGraph();
    }

    private static ResourceGraphBuilder CreateResourceGraphBuilder(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IJsonApiOptions>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        using IServiceScope scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LinkDbContext>();

        var builder = new ResourceGraphBuilder(options, loggerFactory);
        builder.Add(dbContext);

        return builder;
    }
}
