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

public sealed class FilterTests : IClassFixture<IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly QueryStringFakers _fakers = new();

    public FilterTests(IntegrationTestContext<OpenApiStartup<QueryStringDbContext>, QueryStringDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<NodesController>();
    }

    [Fact]
    public async Task Can_filter_in_primary_resources()
    {
        // Arrange
        List<Node> nodes = _fakers.Node.GenerateList(2);
        nodes[0].Name = "John No Quote";
        nodes[1].Name = "Brian O'Quote";

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
            ["filter"] = "equals(name,'Brian O''Quote')"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes.GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(nodes[1].StringId);

            response.Data.ElementAt(0).Attributes.RefShould().NotBeNull().And.Subject.With(attributes =>
            {
                attributes.Name.Should().Be(nodes[1].Name);
                attributes.Comment.Should().Be(nodes[1].Comment);
            });

            response.Meta.Should().NotBeNull();
            response.Meta.AdditionalData.Should().ContainKey("total").WhoseValue.Should().Be(1);
        }
    }

    [Fact]
    public async Task Can_filter_in_secondary_resources()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(2);
        node.Children.ElementAt(0).Comment = "Discount: $10";
        node.Children.ElementAt(1).Comment = "Discount: 5%";

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
            ["filter"] = "and(startsWith(comment,'Discount:'),contains(comment,'%'))"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeCollectionResponseDocument? response = await apiClient.Nodes[node.StringId!].Children.GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(1).StringId);

            response.Data.ElementAt(0).Attributes.RefShould().NotBeNull().And.Subject.With(attributes =>
            {
                attributes.Name.Should().Be(node.Children.ElementAt(1).Name);
                attributes.Comment.Should().Be(node.Children.ElementAt(1).Comment);
            });

            response.Meta.Should().NotBeNull();
            response.Meta.AdditionalData.Should().ContainKey("total").WhoseValue.Should().Be(1);
        }
    }

    [Fact]
    public async Task Can_filter_at_ToMany_relationship_endpoint()
    {
        // Arrange
        Node node = _fakers.Node.GenerateOne();
        node.Children = _fakers.Node.GenerateSet(2);
        node.Children.ElementAt(0).Children = _fakers.Node.GenerateSet(1);
        node.Children.ElementAt(1).Children = _fakers.Node.GenerateSet(2);

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
            ["filter"] = "greaterThan(count(children),'1')"
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            NodeIdentifierCollectionResponseDocument? response = await apiClient.Nodes[node.StringId!].Relationships.Children.GetAsync();

            // Assert
            response.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
            response.Data.ElementAt(0).Id.Should().Be(node.Children.ElementAt(1).StringId);
            response.Meta.Should().NotBeNull();
            response.Meta.AdditionalData.Should().ContainKey("total").WhoseValue.Should().Be(1);
            response.Links.Should().NotBeNull();
            response.Links.Describedby.Should().Be("/swagger/v1/swagger.json");
        }
    }

    [Fact]
    public async Task Cannot_use_empty_filter()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new QueryStringsClient(requestAdapter);

        var queryString = new Dictionary<string, string?>
        {
            ["filter"] = null
        };

        using (_requestAdapterFactory.WithQueryString(queryString))
        {
            // Act
            Func<Task> action = async () => _ = await apiClient.Nodes[Unknown.StringId.Int64].GetAsync();

            // Assert
            ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
            exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
            exception.Links.Should().NotBeNull();
            exception.Links.Describedby.Should().Be("/swagger/v1/swagger.json");
            exception.Errors.Should().HaveCount(1);

            ErrorObject error = exception.Errors[0];
            error.Status.Should().Be("400");
            error.Title.Should().Be("Missing query string parameter value.");
            error.Detail.Should().Be("Missing value for 'filter' query string parameter.");
            error.Source.Should().NotBeNull();
            error.Source.Parameter.Should().Be("filter");
        }
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
