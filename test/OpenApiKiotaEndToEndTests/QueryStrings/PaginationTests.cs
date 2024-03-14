using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode;
using OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.QueryStrings;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.QueryStrings;

public sealed class PaginationTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly QueryStringFakers _fakers = new();

    public PaginationTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_paginate_in_primary_resources()
    {
        // Arrange
        List<Node> nodes = _fakers.Node.Generate(3);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.AddRange(nodes);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = "1",
            ["page[number]"] = "2"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(nodes[1].StringId);
            response.Meta.ShouldNotBeNull();
            response.Meta.AdditionalData.ShouldContainKey("total").With(total => total.Should().Be(3));
        }
    }

    [Fact]
    public async Task Can_paginate_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(3).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = "2",
            ["page[number]"] = "1"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes[node.StringId].Children.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(2);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(0).StringId);
            response.Data.ElementAt(1).Id.Should().Be(node.Children.ElementAt(1).StringId);
            response.Meta.ShouldNotBeNull();
            response.Meta.AdditionalData.ShouldContainKey("total").With(total => total.Should().Be(3));
        }
    }

    [Fact]
    public async Task Can_paginate_at_ToMany_relationship_endpoint()
    {
        // Arrange
        Node node = _fakers.Node.Generate();
        node.Children = _fakers.Node.Generate(3).ToHashSet();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Node>();
            dbContext.Nodes.Add(node);
            await dbContext.SaveChangesAsync();
        });

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = "2",
            ["page[number]"] = "2"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeIdentifierCollectionResponseDocument? response = await apiClient.Nodes[node.StringId].Relationships.Children.GetAsync();

            // Assert
            response.ShouldNotBeNull();
            response.Data.ShouldHaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(2).StringId);
            response.Meta.ShouldNotBeNull();
            response.Meta.AdditionalData.ShouldContainKey("total").With(total => total.Should().Be(3));
        }
    }

    [Fact]
    public async Task Cannot_use_empty_page_size()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["page[size]"] = null
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            Func<Task> action = async () => _ = await apiClient.Nodes[Unknown.StringId.Int64].GetAsync();

            // Assert
            ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
            exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            exception.Errors.ShouldHaveCount(1);

            ErrorObject error = exception.Errors.ElementAt(0);
            error.Status.Should().Be("400");
            error.Title.Should().Be("Missing query string parameter value.");
            error.Detail.Should().Be("Missing value for 'page[size]' query string parameter.");
            error.Source.ShouldNotBeNull();
            error.Source.Parameter.Should().Be("page[size]");
        }
    }

    [Fact]
    public async Task Cannot_use_empty_page_number()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["page[number]"] = null
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            Func<Task> action = async () => _ = await apiClient.Nodes[Unknown.StringId.Int64].GetAsync();

            // Assert
            ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
            exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            exception.Errors.ShouldHaveCount(1);

            ErrorObject error = exception.Errors.ElementAt(0);
            error.Status.Should().Be("400");
            error.Title.Should().Be("Missing query string parameter value.");
            error.Detail.Should().Be("Missing value for 'page[number]' query string parameter.");
            error.Source.ShouldNotBeNull();
            error.Source.Parameter.Should().Be("page[number]");
        }
    }
}
