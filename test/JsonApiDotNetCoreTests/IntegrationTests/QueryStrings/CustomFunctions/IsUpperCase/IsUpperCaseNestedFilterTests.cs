using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

public sealed class IsUpperCaseNestedFilterTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public IsUpperCaseNestedFilterTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, IsUpperCaseFilterParser>();
            services.AddTransient<IWhereClauseBuilder, IsUpperCaseWhereClauseBuilder>();
        });
    }

    [Theory]
    [InlineData("has(someCompound.someStrings,isUpperCase(_))", "1")]
    [InlineData("has(someCompounds,has(someStrings,isUpperCase(_)))", "2")]
    [InlineData("has(someCompounds,isUpperCase(someString))", "2")]
    public async Task Can_filter_casing_on_nested_values(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources =
        [
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeString = "lower",
                    SomeStrings =
                    [
                        "a",
                        "b"
                    ]
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeString = "UPPER",
                    SomeStrings =
                    [
                        "a",
                        "B"
                    ]
                }
            },
            new()
            {
                SomeCompounds =
                [
                    new FilterableNested
                    {
                        SomeString = "UPPER",
                        SomeStrings = ["X"]
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
