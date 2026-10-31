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

    [Theory]
    [InlineData("has(someCompound.someNullableInt32s)", "01")]
    [InlineData("has(someCompound.someNullableInt32s,equals(_,'5'))", "0")]
    [InlineData("has(someCompound.someNullableInt32s,equals(_,null))", "0",
        Skip = "EF Core 10: Comparing an element of a JSON collection with null fails in PostgreSQL (42804: could not determine polymorphic type because input has type unknown).")]
    [InlineData("has(someCompound.child.someTimeSpans,equals(_,'09:00:00'))", "0")]
    [InlineData("has(someCompound.child.someTimeSpans,equals(_,'16:00:00'))", "1")]
    public async Task Can_filter_scalar_collection_nested_in_compound(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources =
        [
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeNullableInt32s =
                    [
                        5,
                        null,
                        4
                    ],
                    Child = new FilterableLeaf
                    {
                        SomeTimeSpans =
                        [
                            TimeSpan.FromHours(9),
                            TimeSpan.FromHours(17)
                        ]
                    }
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeNullableInt32s =
                    [
                        1,
                        2,
                        3
                    ],
                    Child = new FilterableLeaf
                    {
                        SomeTimeSpans =
                        [
                            TimeSpan.FromHours(8),
                            TimeSpan.FromHours(16)
                        ]
                    }
                }
            },
            new()
            {
                SomeCompound = new FilterableNested()
            },
            new()
        ];

        await SeedAsync(resources);

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    [Theory]
    [InlineData("has(someLabeledDates,equals(value,'2020-05-15'))", "0")]
    [InlineData("has(someLabeledDates,equals(value,null))", "")]
    [InlineData("has(someLabeledDates,equals(label,'hired'))", "1")]
    [InlineData("has(someLabeledDates,equals(label,null))", "2")]
    public async Task Can_filter_collection_of_generic_compound(string filterExpression, string expectedRows)
    {
        // Arrange
        List<FilterableResource> resources =
        [
            new()
            {
                SomeLabeledDates =
                [
                    new FilterableLabeled<DateOnly>
                    {
                        Value = new DateOnly(2020, 5, 15),
                        Label = "anniversary"
                    }
                ]
            },
            new()
            {
                SomeLabeledDates =
                [
                    new FilterableLabeled<DateOnly>
                    {
                        Value = new DateOnly(2018, 1, 1),
                        Label = "hired"
                    }
                ]
            },
            new()
            {
                SomeLabeledDates =
                [
                    new FilterableLabeled<DateOnly>
                    {
                        Value = new DateOnly(2019, 3, 3)
                    }
                ]
            }
        ];

        await SeedAsync(resources);

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    [Theory]
    [InlineData("equals(someCompound.child.someString,'Smith')", "0")]
    [InlineData("startsWith(someCompound.child.someString,'Smi')", "02")]
    [InlineData("contains(someCompound.child.someString,'ones')", "1")]
    public async Task Can_filter_on_text_in_nested_compound(string filterExpression, string expectedRows)
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
                        SomeString = "Smith"
                    }
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    Child = new FilterableLeaf
                    {
                        SomeString = "Jones"
                    }
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    Child = new FilterableLeaf
                    {
                        SomeString = "Smithson"
                    }
                }
            },
            new()
            {
                SomeCompound = new FilterableNested()
            }
        ];

        await SeedAsync(resources);

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        AssertRows(responseDocument, resources, expectedRows);
    }

    private async Task SeedAsync(List<FilterableResource> resources)
    {
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });
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
                    SomeStrings =
                    [
                        "a",
                        "b"
                    ],
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
                        SomeStrings =
                        [
                            "a",
                            "b"
                        ]
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
