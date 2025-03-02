using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

public sealed class DocumentDescriptionLinkTests : IClassFixture<IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> _testContext;

    public DocumentDescriptionLinkTests(IntegrationTestContext<TestableStartup<LinksDbContext>, LinksDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<PhotosController>();

        testContext.ConfigureServices(services => services.AddSingleton<IDocumentDescriptionLinkProvider, TestDocumentDescriptionLinkProvider>());
    }

    [Fact]
    public async Task Get_primary_resource_by_ID_converts_relative_documentation_link_to_absolute()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = false;

        var provider = (TestDocumentDescriptionLinkProvider)_testContext.Factory.Services.GetRequiredService<IDocumentDescriptionLinkProvider>();
        provider.Link = "description/json-schema?version=v1.0";

        string route = $"/photos/{Unknown.StringId.For<Photo, Guid>()}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.DescribedBy.Should().Be("http://localhost/description/json-schema?version=v1.0");
    }

    [Fact]
    public async Task Get_primary_resource_by_ID_converts_absolute_documentation_link_to_relative()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;

        var provider = (TestDocumentDescriptionLinkProvider)_testContext.Factory.Services.GetRequiredService<IDocumentDescriptionLinkProvider>();
        provider.Link = "http://localhost:80/description/json-schema?version=v1.0";

        string route = $"/photos/{Unknown.StringId.For<Photo, Guid>()}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.DescribedBy.Should().Be("description/json-schema?version=v1.0");
    }

    [Fact]
    public async Task Get_primary_resource_by_ID_cannot_convert_absolute_documentation_link_to_relative()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = true;

        var provider = (TestDocumentDescriptionLinkProvider)_testContext.Factory.Services.GetRequiredService<IDocumentDescriptionLinkProvider>();
        provider.Link = "https://docs.api.com/description/json-schema?version=v1.0";

        string route = $"/photos/{Unknown.StringId.For<Photo, Guid>()}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.DescribedBy.Should().Be("https://docs.api.com/description/json-schema?version=v1.0");
    }

    private sealed class TestDocumentDescriptionLinkProvider : IDocumentDescriptionLinkProvider
    {
        public string? Link { get; set; }

        public string? GetUrl()
        {
            return Link;
        }
    }
}
