using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

public sealed class LengthNestedFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public LengthNestedFilterTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, LengthFilterParser>();
            services.AddTransient<IWhereClauseBuilder, LengthWhereClauseBuilder>();
        });
    }

    [Theory]
    [InlineData("has(someCompound.someStrings,greaterThan(length(_),'3'))", "1")]
    [InlineData("has(someCompound.someStrings,equals(length(_),'1'))", "01")]
    [InlineData("has(someCompounds,has(someStrings,greaterThan(length(_),'3')))", "2")]
    [InlineData("has(someCompounds,greaterThan(length(someString),'3'))", "2")]
    [InlineData("greaterThan(length(someCompound.someString),'3')", "1")]
    public async Task Can_filter_length_on_nested_values(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources =
        [
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeString = "ab",
                    SomeStrings =
                    [
                        "a",
                        "bb"
                    ]
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeString = "abcd",
                    SomeStrings =
                    [
                        "c",
                        "dddd"
                    ]
                }
            },
            new()
            {
                SomeCompounds =
                [
                    new FilterableNested
                    {
                        SomeString = "wxyz",
                        SomeStrings = ["xxxxx"]
                    }
                ]
            },
            new()
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] expectedIds = expectedRows.Select(row => resources[row - '0'].StringId!).ToArray();
        responseDocument.Data.ManyValue.Should().HaveCount(expectedIds.Length);
        responseDocument.Data.ManyValue.Select(resource => resource.Id).Should().BeEquivalentTo(expectedIds);
    }
}
