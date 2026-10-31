using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.TimeOffset;

public sealed class TimeOffsetNestedTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public TimeOffsetNestedTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        testContext.ConfigureServices(services =>
        {
            services.AddTransient<IFilterParser, TimeOffsetFilterParser>();
            services.AddScoped(typeof(IResourceDefinition<,>), typeof(FilterRewritingResourceDefinition<,>));
        });
    }

    [Fact]
    public async Task Can_filter_on_nested_date_with_offset()
    {
        // Arrange
        var timeProvider = _testContext.App.Services.GetRequiredService<TimeProvider>();
        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        List<FilterableResource> resources =
        [
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeDateTime = utcNow.AddMinutes(-15)
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeDateTime = utcNow.AddMinutes(-5)
                }
            },
            new()
            {
                SomeCompound = new FilterableNested
                {
                    SomeDateTime = utcNow.AddMinutes(5)
                }
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=lessThan(someCompound.someDateTime,timeOffset('-0:10:00'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resources[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_date_with_offset_in_compound_collection_scope()
    {
        // Arrange
        var timeProvider = _testContext.App.Services.GetRequiredService<TimeProvider>();
        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        List<FilterableResource> resources =
        [
            new()
            {
                SomeCompounds =
                [
                    new FilterableNested
                    {
                        SomeDateTime = utcNow.AddMinutes(-15)
                    },
                    new FilterableNested
                    {
                        SomeDateTime = utcNow.AddMinutes(-20)
                    }
                ]
            },
            new()
            {
                SomeCompounds =
                [
                    new FilterableNested
                    {
                        SomeDateTime = utcNow.AddMinutes(-15)
                    },
                    new FilterableNested
                    {
                        SomeDateTime = utcNow.AddMinutes(-5)
                    }
                ]
            }
        ];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=has(someCompounds,lessThan(someDateTime,timeOffset('-0:10:00')))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resources[1].StringId);
    }
}
