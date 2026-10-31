using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Sorting;

/// <summary>
/// Verifies that the AllowSort capability is checked on every attribute in a sort chain, including compound attributes and collections that are the
/// target of count().
/// </summary>
public sealed class SortCapabilityTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public SortCapabilityTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();
    }

    [Theory]
    [InlineData("^someUnsortableCompound.someInt32", "someUnsortableCompound")]
    [InlineData("-^someUnsortableCompound.someInt32", "someUnsortableCompound")]
    [InlineData("someInt32,^someUnsortableCompound.someString", "someUnsortableCompound")]
    [InlineData("someCompound.^unsortableChild.someInt32", "unsortableChild")]
    [InlineData("count(^someUnsortableCompounds)", "someUnsortableCompounds")]
    [InlineData("-count(^someUnsortableCompounds)", "someUnsortableCompounds")]
    [InlineData("count(^someUnsortableStrings)", "someUnsortableStrings")]
    [InlineData("count(someCompound.^unsortableLeaves)", "unsortableLeaves")]
    [InlineData("count(someCompound.^unsortableStrings)", "unsortableStrings")]
    [InlineData("count(someCompound.^unsortableChild.someTimeSpans)", "unsortableChild")]
    public async Task Cannot_sort_when_attribute_in_chain_has_blocked_capability(string sortValue, string attributeName)
    {
        // Arrange
        var parameterValue = new MarkedText(sortValue, '^');
        string route = $"/filterableResources?sort={parameterValue.Text}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        ErrorObject error = responseDocument.Errors.Should().ContainSingle().Which;
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().Be($"Sorting on attribute '{attributeName}' is not allowed. {parameterValue}");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("sort");
    }

    [Theory]
    [InlineData("someUnsortableCompound", "Field chain on resource type 'filterableResources' failed to match the pattern: ")]
    [InlineData("someUnsortableCompounds", "Field chain on resource type 'filterableResources' failed to match the pattern: ")]
    [InlineData("someUnsortableStrings", "Field chain on resource type 'filterableResources' failed to match the pattern: ")]
    [InlineData("count(someUnsortableCompound)", "Field chain on resource type 'filterableResources' failed to match the pattern: ")]
    public async Task Cannot_sort_on_compound_or_collection_without_count(string sortValue, string errorPrefix)
    {
        // Arrange
        string route = $"/filterableResources?sort={sortValue}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        ErrorObject error = responseDocument.Errors.Should().ContainSingle().Which;
        error.Title.Should().Be("The specified sort is invalid.");
        error.Detail.Should().StartWith(errorPrefix);
    }

    [Fact]
    public async Task Can_sort_on_attributes_and_counts_with_default_capabilities()
    {
        // Arrange
        List<FilterableResource> resources =
        [
            new()
            {
                SomeCompound = new FilterableNested
                {
                    Child = new FilterableLeaf
                    {
                        SomeInt32 = 3
                    },
                    SomeStrings = ["a"]
                },
                SomeCompounds =
                [
                    new FilterableNested(),
                    new FilterableNested()
                ]
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    Child = new FilterableLeaf
                    {
                        SomeInt32 = 1
                    },
                    SomeStrings =
                    [
                        "a",
                        "b",
                        "c"
                    ]
                },
                SomeCompounds = [new FilterableNested()]
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    Child = new FilterableLeaf
                    {
                        SomeInt32 = 2
                    },
                    SomeStrings =
                    [
                        "a",
                        "b"
                    ]
                }
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage byChildResponse, Document byChildDocument) =
            await _testContext.ExecuteGetAsync<Document>("/filterableResources?sort=someCompound.child.someInt32");

        (HttpResponseMessage byCompoundCountResponse, Document byCompoundCountDocument) =
            await _testContext.ExecuteGetAsync<Document>("/filterableResources?sort=-count(someCompounds),id");

        (HttpResponseMessage byScalarCountResponse, Document byScalarCountDocument) =
            await _testContext.ExecuteGetAsync<Document>("/filterableResources?sort=-count(someCompound.someStrings)");

        // Assert
        byChildResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        byChildDocument.GetPrimaryIds().Should().Equal(resources[1].StringId, resources[2].StringId, resources[0].StringId);

        byCompoundCountResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        byCompoundCountDocument.GetPrimaryIds().Should().Equal(resources[0].StringId, resources[1].StringId, resources[2].StringId);

        byScalarCountResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        byScalarCountDocument.GetPrimaryIds().Should().Equal(resources[1].StringId, resources[2].StringId, resources[0].StringId);
    }
}
