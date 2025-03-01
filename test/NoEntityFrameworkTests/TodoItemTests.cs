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

public sealed class TodoItemTests(NoLoggingWebApplicationFactory<TodoItem> factory) : IntegrationTest, IClassFixture<NoLoggingWebApplicationFactory<TodoItem>>
{
    private readonly NoLoggingWebApplicationFactory<TodoItem> _factory = factory;

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
        const string route = "/api/todoItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);

        responseDocument.Meta.Should().ContainTotal(4);
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        const string route = "/api/todoItems?filter=equals(priority,'High')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("priority").With(value => value.Should().Be(TodoItemPriority.High));

        responseDocument.Meta.Should().ContainTotal(1);
    }

    [Fact]
    public async Task Can_filter_in_related_resources()
    {
        // Arrange
        const string route = "/api/todoItems?filter=not(equals(assignee.firstName,'Jane'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);

        responseDocument.Meta.Should().ContainTotal(3);
    }

    [Fact]
    public async Task Can_sort_on_attribute_in_primary_resources()
    {
        // Arrange
        const string route = "/api/todoItems?sort=-id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);
        responseDocument.Data.ManyValue[0].Id.Should().Be("4");
        responseDocument.Data.ManyValue[1].Id.Should().Be("3");
        responseDocument.Data.ManyValue[2].Id.Should().Be("2");
        responseDocument.Data.ManyValue[3].Id.Should().Be("1");
    }

    [Fact]
    public async Task Can_sort_on_count_in_primary_resources()
    {
        // Arrange
        const string route = "/api/todoItems?sort=count(assignee.ownedTodoItems)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(4);
        responseDocument.Data.ManyValue[0].Id.Should().Be("2");
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        const string route = "/api/todoItems?page[size]=3&page[number]=2&sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("description").With(value => value.Should().Be("Check emails"));

        responseDocument.Meta.Should().ContainTotal(4);
    }

    [Fact]
    public async Task Can_select_fields_in_primary_resources()
    {
        // Arrange
        const string route = "/api/todoItems?fields[todoItems]=description,priority";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().NotBeEmpty();
        responseDocument.Data.ManyValue.Should().AllSatisfy(resource => resource.Attributes.ShouldOnlyContainKeys("description", "priority"));
    }

    [Fact]
    public async Task Can_include_in_primary_resources()
    {
        // Arrange
        const string route = "/api/todoItems?include=owner.assignedTodoItems,assignee,tags";

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
        const string route = "/api/todoItems/1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be("1");
    }

    [Fact]
    public async Task Cannot_get_primary_resource_for_unknown_ID()
    {
        // Arrange
        const string route = "/api/todoItems/999999";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be("Resource of type 'todoItems' with ID '999999' does not exist.");
    }

    [Fact]
    public async Task Can_get_secondary_resources()
    {
        // Arrange
        const string route = "/api/todoItems/3/tags?sort=id";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("name").With(value => value.Should().Be("Personal"));
        responseDocument.Data.ManyValue[1].Attributes.ShouldContainKey("name").With(value => value.Should().Be("Family"));

        responseDocument.Meta.Should().ContainTotal(2);
    }

    [Fact]
    public async Task Can_get_secondary_resource()
    {
        // Arrange
        const string route = "/api/todoItems/2/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be("Jane"));
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_unknown_primary_ID()
    {
        // Arrange
        const string route = "/api/todoItems/999999/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be("Resource of type 'todoItems' with ID '999999' does not exist.");
    }

    [Fact]
    public async Task Can_get_secondary_resource_for_unknown_secondary_ID()
    {
        // Arrange
        const string route = "/api/todoItems/2/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_get_secondary_resource_for_unknown_relationship()
    {
        // Arrange
        const string route = "/api/todoItems/2/unknown";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested relationship does not exist.");
        error.Detail.Should().Be("Resource of type 'todoItems' does not contain a relationship named 'unknown'.");
    }

    [Fact]
    public async Task Can_get_ToOne_relationship()
    {
        // Arrange
        const string route = "/api/todoItems/2/relationships/owner";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("people");
        responseDocument.Data.SingleValue.Id.Should().Be("2");
    }

    [Fact]
    public async Task Can_get_empty_ToOne_relationship()
    {
        // Arrange
        const string route = "/api/todoItems/2/relationships/assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_ToMany_relationship()
    {
        // Arrange
        const string route = "/api/todoItems/4/relationships/tags?sort=id";

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
