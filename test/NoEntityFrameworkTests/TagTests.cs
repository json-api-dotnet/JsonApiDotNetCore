using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using NoEntityFrameworkExample.Models;
using TestBuildingBlocks;
using Xunit;

namespace NoEntityFrameworkTests;

public sealed class TagTests(NoLoggingWebApplicationFactory<Tag> factory) : IntegrationTest, IClassFixture<NoLoggingWebApplicationFactory<Tag>>
{
    private readonly NoLoggingWebApplicationFactory<Tag> _factory = factory;

    protected override JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = _factory.Services.GetRequiredService<IJsonApiOptions>();
            return options.SerializerOptions;
        }
    }

    [Fact]
    public async Task Can_get_primary_resources()
    {
        // Arrange
        const string route = "/api/tags";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Meta.Should().ContainTotal(3);
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        const string route = "/api/tags?filter=equals(name,'Personal')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be("Personal"));

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Can_filter_in_related_resources()
    {
        // Arrange
        const string route = "/api/tags?filter=has(todoItems,equals(description,'Check emails'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be("Business"));

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Can_sort_on_attribute_in_primary_resources()
    {
        // Arrange
        const string route = "/api/tags?sort=-id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be("3");
        responseDocument.Data.ManyValue[1].Id.Should().Be("2");
        responseDocument.Data.ManyValue[2].Id.Should().Be("1");
    }

    [Fact]
    public async Task Can_sort_on_count_in_primary_resources()
    {
        // Arrange
        const string route = "/api/tags?sort=-count(todoItems),id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be("1");
        responseDocument.Data.ManyValue[1].Id.Should().Be("2");
        responseDocument.Data.ManyValue[2].Id.Should().Be("3");
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        const string route = "/api/tags?page[size]=1&page[number]=2&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be("Family"));

        responseDocument.Meta.Should().ContainTotal(3);
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resources()
    {
        // Arrange
        const string route = "/api/tags?fields[tags]=todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().NotBeEmpty();
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Attributes.Should().BeNull());
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Relationships.Should().OnlyContainKeys("todoItems"));
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        const string route = "/api/tags?include=todoItems.owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().NotBeEmpty();
        responseDocument.Included.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Can_get_primary_resource()
    {
        // Arrange
        const string route = "/api/tags/1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be("1");
    }

    [Fact]
    public async Task Can_get_secondary_resources()
    {
        // Arrange
        const string route = "/api/tags/1/todoItems?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("description").With(value => value.Should().Be("Make homework"));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("description").With(value => value.Should().Be("Book vacation"));
        responseDocument.Data.ManyValue[2].Attributes.ShouldContainKey("description").With(value => value.Should().Be("Cook dinner"));

        responseDocument.Meta.Should().ContainTotal(3);
    }

    [Fact]
    public async Task Can_get_ToMany_relationship()
    {
        // Arrange
        const string route = "/api/tags/2/relationships/todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be("3");

        responseDocument.Meta.Should().ContainTotal(1);
    }

    protected override HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }
}
