using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

/// <summary>
/// Verifies comparisons against null on elements of scalar collections whose element type is a non-nullable value type.
/// </summary>
public sealed class FilterCompoundNullTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public FilterCompoundNullTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = false;
    }

    [Theory]
    [InlineData("has(someGuids,equals(_,null))", "")]
    [InlineData("not(has(someGuids,equals(_,null)))", "01")]
    [InlineData("has(someTimeSpans,equals(_,null))", "")]
    [InlineData("not(has(someTimeSpans,equals(_,null)))", "01")]
    [InlineData("has(someGuids,not(equals(_,null)))", "01")]
    [InlineData("has(someTimeSpans,not(equals(_,null)))", "01")]
    public async Task Can_compare_elements_of_non_nullable_collection_with_null(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources =
        [
            new()
            {
                SomeGuids = [Guid.NewGuid()],
                SomeTimeSpans = [TimeSpan.FromHours(1)]
            },
            new()
            {
                SomeGuids =
                [
                    Guid.NewGuid(),
                    Guid.NewGuid()
                ],
                SomeTimeSpans =
                [
                    TimeSpan.FromHours(2),
                    TimeSpan.FromHours(3)
                ]
            }
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
