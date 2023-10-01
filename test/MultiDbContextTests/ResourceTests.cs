using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MultiDbContextExample.Models;
using TestBuildingBlocks;
using Xunit;

namespace MultiDbContextTests;

public sealed class ResourceTests : IntegrationTest, IClassFixture<NoLoggingWebApplicationFactory<ResourceA>>
{
    private readonly NoLoggingWebApplicationFactory<ResourceA> _factory;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = _factory.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    public ResourceTests(NoLoggingWebApplicationFactory<ResourceA> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Can_get_ResourceAs()
    {
        // Arrange
        const string route = "/api/resourceAs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("nameA").With(value => value.Should().Be("SampleA"));
    }

    [Fact]
    public async Task Can_get_ResourceBs()
    {
        // Arrange
        const string route = "/api/resourceBs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("nameB").With(value => value.Should().Be("SampleB"));
    }

    protected override HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }
}
