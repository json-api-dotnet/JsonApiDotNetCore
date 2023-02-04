using System.Globalization;
using System.Net;
using System.Web;
using FluentAssertions;
using FluentAssertions.Extensions;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterOperatorTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private const string IntLowerBound = "19";
    private const string IntInTheRange = "20";
    private const string IntUpperBound = "21";

    private const string DoubleLowerBound = "1.9";
    private const string DoubleInTheRange = "2.0";
    private const string DoubleUpperBound = "2.1";

    private const string IsoDateTimeLowerBound = "2000-11-22T09:48:17";
    private const string IsoDateTimeInTheRange = "2000-11-22T12:34:56";
    private const string IsoDateTimeUpperBound = "2000-11-22T18:47:32";

    private const string InvariantDateTimeLowerBound = "11/22/2000 9:48:17";
    private const string InvariantDateTimeInTheRange = "11/22/2000 12:34:56";
    private const string InvariantDateTimeUpperBound = "11/22/2000 18:47:32";

    private const string TimeSpanLowerBound = "2:15:28:54.997";
    private const string TimeSpanInTheRange = "2:15:51:42.397";
    private const string TimeSpanUpperBound = "2:16:22:41.736";

    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public FilterOperatorTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = false;
    }

    [Fact]
    public async Task Can_filter_equality_on_special_characters()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeString = "This, that & more + some"
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someString,'{HttpUtility.UrlEncode(resource.SomeString)}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someString").With(value => value.Should().Be(resource.SomeString));
    }

    [Fact]
    public async Task Can_filter_equality_on_two_attributes_of_same_type()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = 5,
            OtherInt32 = 5
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = 5,
            OtherInt32 = 10
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(someInt32,otherInt32)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someInt32").With(value => value.Should().Be(resource.SomeInt32));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("otherInt32").With(value => value.Should().Be(resource.OtherInt32));
    }

    [Fact]
    public async Task Can_filter_equality_on_two_attributes_of_same_nullable_type()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeNullableInt32 = 5,
            OtherNullableInt32 = 5
        };

        var otherResource = new FilterableResource
        {
            SomeNullableInt32 = 5,
            OtherNullableInt32 = 10
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(someNullableInt32,otherNullableInt32)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someNullableInt32").With(value => value.Should().Be(resource.SomeNullableInt32));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("otherNullableInt32").With(value => value.Should().Be(resource.OtherNullableInt32));
    }

    [Fact]
    public async Task Can_filter_equality_on_two_attributes_with_nullable_at_start()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = 5,
            SomeNullableInt32 = 5
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = 5,
            SomeNullableInt32 = 10
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(someNullableInt32,someInt32)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someInt32").With(value => value.Should().Be(resource.SomeInt32));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someNullableInt32").With(value => value.Should().Be(resource.SomeNullableInt32));
    }

    [Fact]
    public async Task Can_filter_equality_on_two_attributes_with_nullable_at_end()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = 5,
            SomeNullableInt32 = 5
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = 5,
            SomeNullableInt32 = 10
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(someInt32,someNullableInt32)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someInt32").With(value => value.Should().Be(resource.SomeInt32));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someNullableInt32").With(value => value.Should().Be(resource.SomeNullableInt32));
    }

    [Fact]
    public async Task Can_filter_equality_on_two_attributes_of_compatible_types()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = 5,
            SomeUnsignedInt64 = 5
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = 5,
            SomeUnsignedInt64 = 10
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(someInt32,someUnsignedInt64)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someInt32").With(value => value.Should().Be(resource.SomeInt32));
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someUnsignedInt64").With(value => value.Should().Be(resource.SomeUnsignedInt64));
    }

    [Fact]
    public async Task Cannot_filter_equality_on_two_attributes_of_incompatible_types()
    {
        // Arrange
        const string route = "/filterableResources?filter=equals(someDouble,someTimeSpan)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Query creation failed due to incompatible types.");
        error.Detail.Should().Be("No coercion operator is defined between types 'System.TimeSpan' and 'System.Double'.");
        error.Source.Should().BeNull();
    }

    [Theory]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessThan, IntInTheRange)]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessThan, IntUpperBound)]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessOrEqual, IntInTheRange)]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessOrEqual, IntLowerBound)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterThan, IntInTheRange)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterThan, IntLowerBound)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterOrEqual, IntInTheRange)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterOrEqual, IntUpperBound)]
    public async Task Can_filter_comparison_on_whole_number(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = int.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = int.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someInt32,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someInt32").With(value => value.Should().Be(resource.SomeInt32));
    }

    [Theory]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessThan, DoubleInTheRange)]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessThan, DoubleUpperBound)]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessOrEqual, DoubleInTheRange)]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessOrEqual, DoubleLowerBound)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterThan, DoubleInTheRange)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterThan, DoubleLowerBound)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterOrEqual, DoubleInTheRange)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterOrEqual, DoubleUpperBound)]
    public async Task Can_filter_comparison_on_fractional_number(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDouble = double.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeDouble = double.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDouble,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDouble").With(value => value.Should().Be(resource.SomeDouble));
    }

    [Theory]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeUpperBound)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeUpperBound)]
    public async Task Can_filter_comparison_on_DateTime_in_local_time_zone(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeInLocalZone = DateTime.Parse(matchingValue, CultureInfo.InvariantCulture).AsLocal()
        };

        var otherResource = new FilterableResource
        {
            SomeDateTimeInLocalZone = DateTime.Parse(nonMatchingValue, CultureInfo.InvariantCulture).AsLocal()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTimeInLocalZone,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeInLocalZone")
            .With(value => value.Should().Be(resource.SomeDateTimeInLocalZone));
    }

    [Theory]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeUpperBound)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeUpperBound)]
    public async Task Can_filter_comparison_on_DateTime_in_UTC_time_zone(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeInUtcZone = DateTime.Parse(matchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        var otherResource = new FilterableResource
        {
            SomeDateTimeInUtcZone = DateTime.Parse(nonMatchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTimeInUtcZone,'{filterValue}Z')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeInUtcZone")
            .With(value => value.Should().Be(resource.SomeDateTimeInUtcZone));
    }

    [Theory]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeUpperBound)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeUpperBound)]
    public async Task Can_filter_comparison_on_DateTimeOffset(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeOffset = DateTime.Parse(matchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        var otherResource = new FilterableResource
        {
            SomeDateTimeOffset = DateTime.Parse(nonMatchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTimeOffset,'{filterValue}Z')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeOffset").With(value => value.Should().Be(resource.SomeDateTimeOffset));
    }

    [Theory]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessThan, TimeSpanInTheRange)]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessThan, TimeSpanUpperBound)]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessOrEqual, TimeSpanInTheRange)]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessOrEqual, TimeSpanLowerBound)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterThan, TimeSpanInTheRange)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterThan, TimeSpanLowerBound)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterOrEqual, TimeSpanInTheRange)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterOrEqual, TimeSpanUpperBound)]
    public async Task Can_filter_comparison_on_TimeSpan(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator, string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeTimeSpan = TimeSpan.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeTimeSpan = TimeSpan.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someTimeSpan,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someTimeSpan").With(value => value.Should().Be(resource.SomeTimeSpan));
    }

    [Theory]
    [InlineData("The fox jumped over the lazy dog", "Other", TextMatchKind.Contains, "jumped")]
    [InlineData("The fox jumped over the lazy dog", "the fox...", TextMatchKind.Contains, "The")]
    [InlineData("The fox jumped over the lazy dog", "The fox jumped", TextMatchKind.Contains, "dog")]
    [InlineData("The fox jumped over the lazy dog", "Yesterday The fox...", TextMatchKind.StartsWith, "The")]
    [InlineData("The fox jumped over the lazy dog", "over the lazy dog earlier", TextMatchKind.EndsWith, "dog")]
    public async Task Can_filter_text_match(string matchingText, string nonMatchingText, TextMatchKind matchKind, string filterText)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeString = matchingText
        };

        var otherResource = new FilterableResource
        {
            SomeString = nonMatchingText
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={matchKind.ToString().Camelize()}(someString,'{filterText}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someString").With(value => value.Should().Be(resource.SomeString));
    }

    [Theory]
    [InlineData("yes", "no", "'yes'")]
    [InlineData("two", "one two", "'one','two','three'")]
    [InlineData("two", "nine", "'one','two','three','four','five'")]
    public async Task Can_filter_in_set(string matchingText, string nonMatchingText, string filterText)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeString = matchingText
        };

        var otherResource = new FilterableResource
        {
            SomeString = nonMatchingText
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=any(someString,{filterText})";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someString").With(value => value.Should().Be(resource.SomeString));
    }

    [Fact]
    public async Task Can_filter_on_has()
    {
        // Arrange
        var resource = new FilterableResource
        {
            Children = new List<FilterableResource>
            {
                new()
            }
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=has(children)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resource.StringId);
    }

    [Fact]
    public async Task Can_filter_on_has_with_nested_condition()
    {
        // Arrange
        var resources = new List<FilterableResource>
        {
            new()
            {
                Children = new List<FilterableResource>
                {
                    new()
                    {
                        SomeBoolean = false
                    }
                }
            },
            new()
            {
                Children = new List<FilterableResource>
                {
                    new()
                    {
                        SomeBoolean = true
                    }
                }
            }
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resources);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=has(children,equals(someBoolean,'true'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resources[1].StringId);
    }

    [Fact]
    public async Task Can_filter_on_count()
    {
        // Arrange
        var resource = new FilterableResource
        {
            Children = new List<FilterableResource>
            {
                new(),
                new()
            }
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(count(children),'2')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resource.StringId);
    }

    [Theory]
    [InlineData("and(equals(someString,'ABC'),equals(someInt32,'11'))")]
    [InlineData("and(equals(someString,'ABC'),equals(someInt32,'11'),equals(someEnum,'Tuesday'))")]
    [InlineData("or(equals(someString,'---'),lessThan(someInt32,'33'))")]
    [InlineData("not(equals(someEnum,'Saturday'))")]
    public async Task Can_filter_on_logical_functions(string filterExpression)
    {
        // Arrange
        var resource1 = new FilterableResource
        {
            SomeString = "ABC",
            SomeInt32 = 11,
            SomeEnum = DayOfWeek.Tuesday
        };

        var resource2 = new FilterableResource
        {
            SomeString = "XYZ",
            SomeInt32 = 99,
            SomeEnum = DayOfWeek.Saturday
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource1, resource2);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resource1.StringId);
    }
}
