using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterCompoundTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public FilterCompoundTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = false;
    }

    [Theory]
    [InlineData("equals(someCompound,null)", "0")]
    [InlineData("not(equals(someCompound,null))", "12")]
    [InlineData("equals(someRequiredCompound,null)", "")]
    [InlineData("equals(someCompounds,null)", "")]
    [InlineData("equals(someStrings,null)", "0")]
    [InlineData("equals(someCompound.someNullableString,null)", "01")]
    [InlineData("equals(someCompound.child,null)", "01")]
    public async Task Can_filter_null_checks(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources = await SeedAsync();

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    [Fact]
    public async Task Null_equality_on_non_nullable_returns_no_rows()
    {
        // Arrange
        await SeedAsync();

        const string route = "/filterableResources?filter=equals(someInt32,null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().BeEmpty();
    }

    [Theory]
    [InlineData("has(someCompounds)", "12")]
    [InlineData("has(someStrings)", "1")]
    public async Task Can_filter_has_without_predicate(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources = await SeedAsync();

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    [Theory]
    [InlineData("has(someCompounds,equals(someString,'x'))", "1")]
    [InlineData("has(someCompounds,contains(someString,'yz'))", "2")]
    [InlineData("has(someCompounds,any(someString,'a','q'))", "2")]
    [InlineData("has(someCompounds,not(equals(someString,'x')))", "2")]
    [InlineData("has(someCompounds,and(equals(someString,'a'),equals(someNullableString,null)))", "2")]
    [InlineData("has(someCompounds,or(equals(someString,'x'),equals(someString,'a')))", "12")]
    public async Task Can_filter_has_with_predicate_on_compound_collection(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources = await SeedAsync();

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    [Theory]
    [InlineData("has(someCompounds,has(someStrings,equals(_,'x')))", "1")]
    [InlineData("has(someCompounds,has(someStrings,equals(_,'b')))", "2")]
    public async Task Can_filter_has_nested_in_compound_collection(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources = await SeedAsync();

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    [Theory]
    [InlineData("equals(count(someCompounds),'0')", "0")]
    [InlineData("greaterThan(count(someCompounds),'1')", "2")]
    [InlineData("lessThan(count(someCompounds),'2')", "01")]
    [InlineData("equals(count(someStrings),'0')", "2")]
    [InlineData("greaterThan(count(someStrings),'0')", "1")]
    [InlineData("lessThan(count(someStrings),'2')", "12")]
    [InlineData("equals(count(someCompound.someStrings),'0')", "1")]
    [InlineData("greaterThan(count(someCompound.someStrings),'1')", "2")]
    [InlineData("lessThan(count(someCompound.someStrings),'3')", "12")]
    public async Task Can_filter_by_count(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources = await SeedAsync();

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    private async Task<List<FilterableResource>> SeedAsync()
    {
        List<FilterableResource> resources =
        [
            new(),
            new()
            {
                SomeCompound = new FilterableNested(),
                SomeCompounds =
                [
                    new FilterableNested
                    {
                        SomeString = "x",
                        SomeStrings = ["x"]
                    }
                ],
                SomeStrings = ["x"]
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeNullableString = "n",
                    SomeStrings = ["a", "b"],
                    Child = new FilterableLeaf
                    {
                        SomeInt32 = 5
                    }
                },
                SomeCompounds =
                [
                    new FilterableNested
                    {
                        SomeString = "a",
                        SomeStrings = ["a", "b"]
                    },
                    new FilterableNested
                    {
                        SomeString = "xyz",
                        SomeNullableString = "q"
                    }
                ],
                SomeStrings = []
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });

        return resources;
    }

    private static void AssertRows(Document responseDocument, List<FilterableResource> resources, string expectedRows)
    {
        string[] expectedIds = expectedRows.Select(row => resources[row - '0'].StringId!).ToArray();

        responseDocument.Data.ManyValue.Should().HaveCount(expectedIds.Length);
        responseDocument.Data.ManyValue.Select(resource => resource.Id).Should().BeEquivalentTo(expectedIds);
    }
}
