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

public sealed class PersonTests(NoLoggingWebApplicationFactory<Person> factory) : IntegrationTest, IClassFixture<NoLoggingWebApplicationFactory<Person>>
{
    private readonly NoLoggingWebApplicationFactory<Person> _factory = factory;

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
        const string route = "/api/people";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);

        responseDocument.Meta.Should().ContainTotal(2);
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        const string route = "/api/people?filter=equals(firstName,'Jane')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("firstName").With(value => value.Should().Be("Jane"));

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Can_filter_in_related_resources()
    {
        // Arrange
        const string route = "/api/people?filter=has(assignedTodoItems,equals(description,'Check emails'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("firstName").With(value => value.Should().Be("John"));

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Can_sort_on_attribute_in_primary_resources()
    {
        // Arrange
        const string route = "/api/people?sort=-id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be("2");
        responseDocument.Data.ManyValue[1].Id.Should().Be("1");
    }

    [Fact]
    public async Task Can_sort_on_count_in_primary_resources()
    {
        // Arrange
        const string route = "/api/people?sort=-count(assignedTodoItems)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be("1");
        responseDocument.Data.ManyValue[1].Id.Should().Be("2");
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        const string route = "/api/people?page[size]=1&page[number]=2&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("firstName").With(value => value.Should().Be("Jane"));

        responseDocument.Meta.Should().ContainTotal(2);
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resources()
    {
        // Arrange
        const string route = "/api/people?fields[people]=lastName,displayName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldNotBeEmpty();
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Attributes.ShouldOnlyContainKeys("lastName", "displayName"));
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        const string route = "/api/people?include=ownedTodoItems.assignee,assignedTodoItems";

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
        const string route = "/api/people/1";

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
        const string route = "/api/people/1/ownedTodoItems?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(2);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("description").With(value => value.Should().Be("Make homework"));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("description").With(value => value.Should().Be("Check emails"));

        responseDocument.Meta.Should().ContainTotal(2);
    }

    [Fact]
    public async Task Can_get_ToMany_relationship()
    {
        // Arrange
        const string route = "/api/people/2/relationships/assignedTodoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be("1");

        responseDocument.Meta.Should().ContainTotal(1);
    }

    protected override HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }
}
