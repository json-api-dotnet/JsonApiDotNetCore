using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;
using FluentAssertions;
using FluentAssertions.Extensions;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterDataTypeTests : IClassFixture<IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

    public FilterDataTypeTests(IntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.EnableLegacyFilterNotation = false;

        if (!options.SerializerOptions.Converters.Any(converter => converter is JsonStringEnumConverter))
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }
    }

    [Theory]
    [InlineData(nameof(FilterableResource.SomeString), "text")]
    [InlineData(nameof(FilterableResource.SomeNullableString), "text")]
    [InlineData(nameof(FilterableResource.SomeBoolean), true)]
    [InlineData(nameof(FilterableResource.SomeNullableBoolean), true)]
    [InlineData(nameof(FilterableResource.SomeInt32), 1)]
    [InlineData(nameof(FilterableResource.SomeNullableInt32), 1)]
    [InlineData(nameof(FilterableResource.SomeUnsignedInt64), 1ul)]
    [InlineData(nameof(FilterableResource.SomeNullableUnsignedInt64), 1ul)]
    [InlineData(nameof(FilterableResource.SomeDouble), 0.5d)]
    [InlineData(nameof(FilterableResource.SomeNullableDouble), 0.5d)]
    [InlineData(nameof(FilterableResource.SomeEnum), DayOfWeek.Saturday)]
    [InlineData(nameof(FilterableResource.SomeNullableEnum), DayOfWeek.Saturday)]
    public async Task Can_filter_equality_on_type(string propertyName, object propertyValue)
    {
        // Arrange
        var resource = new FilterableResource();
        PropertyInfo? property = typeof(FilterableResource).GetProperty(propertyName);
        property?.SetValue(resource, propertyValue);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string attributeName = propertyName.Camelize();
        string route = $"/filterableResources?filter=equals({attributeName},'{propertyValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey(attributeName).With(value => value.Should().Be(value));
    }

    [Fact]
    public async Task Can_filter_equality_on_type_Decimal()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDecimal = 0.5m
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someDecimal,'{resource.SomeDecimal}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDecimal").With(value => value.Should().Be(resource.SomeDecimal));
    }

    [Fact]
    public async Task Can_filter_equality_on_type_Guid()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeGuid = Guid.NewGuid()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someGuid,'{resource.SomeGuid}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someGuid").With(value => value.Should().Be(resource.SomeGuid));
    }

    [Fact]
    public async Task Can_filter_equality_on_type_DateTime_in_local_time_zone()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeInLocalZone = 27.January(2003).At(11, 22, 33, 44)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someDateTimeInLocalZone,'{resource.SomeDateTimeInLocalZone:O}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeInLocalZone")
            .With(value => value.Should().Be(resource.SomeDateTimeInLocalZone));
    }

    [Fact]
    public async Task Can_filter_equality_on_type_DateTime_in_UTC_time_zone()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeInUtcZone = 27.January(2003).At(11, 22, 33, 44).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someDateTimeInUtcZone,'{resource.SomeDateTimeInUtcZone:O}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeInUtcZone")
            .With(value => value.Should().Be(resource.SomeDateTimeInUtcZone));
    }

    [Fact]
    public async Task Can_filter_equality_on_type_DateTimeOffset_in_UTC_time_zone()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeOffset = 27.January(2003).At(11, 22, 33, 44).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someDateTimeOffset,'{HttpUtility.UrlEncode(resource.SomeDateTimeOffset.ToString("O"))}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeOffset").With(value => value.Should().Be(resource.SomeDateTimeOffset));
    }

    [Fact]
    public async Task Can_filter_equality_on_type_TimeSpan()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeTimeSpan = new TimeSpan(1, 2, 3, 4, 5)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someTimeSpan,'{resource.SomeTimeSpan}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someTimeSpan").With(value => value.Should().Be(resource.SomeTimeSpan));
    }

    [Fact]
    public async Task Cannot_filter_equality_on_incompatible_value()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = 1
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        const string route = "/filterableResources?filter=equals(someInt32,'ABC')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Query creation failed due to incompatible types.");
        error.Detail.Should().Be("Failed to convert 'ABC' of type 'String' to type 'Int32'.");
        error.Source.Should().BeNull();
    }

    [Theory]
    [InlineData(nameof(FilterableResource.SomeNullableString))]
    [InlineData(nameof(FilterableResource.SomeNullableBoolean))]
    [InlineData(nameof(FilterableResource.SomeNullableInt32))]
    [InlineData(nameof(FilterableResource.SomeNullableUnsignedInt64))]
    [InlineData(nameof(FilterableResource.SomeNullableDecimal))]
    [InlineData(nameof(FilterableResource.SomeNullableDouble))]
    [InlineData(nameof(FilterableResource.SomeNullableGuid))]
    [InlineData(nameof(FilterableResource.SomeNullableDateTime))]
    [InlineData(nameof(FilterableResource.SomeNullableDateTimeOffset))]
    [InlineData(nameof(FilterableResource.SomeNullableTimeSpan))]
    [InlineData(nameof(FilterableResource.SomeNullableEnum))]
    public async Task Can_filter_is_null_on_type(string propertyName)
    {
        // Arrange
        var resource = new FilterableResource();
        PropertyInfo? property = typeof(FilterableResource).GetProperty(propertyName);
        property?.SetValue(resource, null);

        var otherResource = new FilterableResource
        {
            SomeNullableString = "X",
            SomeNullableBoolean = true,
            SomeNullableInt32 = 1,
            SomeNullableUnsignedInt64 = 1,
            SomeNullableDecimal = 1,
            SomeNullableDouble = 1,
            SomeNullableGuid = Guid.NewGuid(),
            SomeNullableDateTime = 1.January(2001).AsUtc(),
            SomeNullableDateTimeOffset = 1.January(2001).AsUtc(),
            SomeNullableTimeSpan = TimeSpan.FromHours(1),
            SomeNullableEnum = DayOfWeek.Friday
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string attributeName = propertyName.Camelize();
        string route = $"/filterableResources?filter=equals({attributeName},null)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey(attributeName).With(value => value.Should().BeNull());
    }

    [Theory]
    [InlineData(nameof(FilterableResource.SomeNullableString))]
    [InlineData(nameof(FilterableResource.SomeNullableBoolean))]
    [InlineData(nameof(FilterableResource.SomeNullableInt32))]
    [InlineData(nameof(FilterableResource.SomeNullableUnsignedInt64))]
    [InlineData(nameof(FilterableResource.SomeNullableDecimal))]
    [InlineData(nameof(FilterableResource.SomeNullableDouble))]
    [InlineData(nameof(FilterableResource.SomeNullableGuid))]
    [InlineData(nameof(FilterableResource.SomeNullableDateTime))]
    [InlineData(nameof(FilterableResource.SomeNullableDateTimeOffset))]
    [InlineData(nameof(FilterableResource.SomeNullableTimeSpan))]
    [InlineData(nameof(FilterableResource.SomeNullableEnum))]
    public async Task Can_filter_is_not_null_on_type(string propertyName)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeNullableString = "X",
            SomeNullableBoolean = true,
            SomeNullableInt32 = 1,
            SomeNullableUnsignedInt64 = 1,
            SomeNullableDecimal = 1,
            SomeNullableDouble = 1,
            SomeNullableGuid = Guid.NewGuid(),
            SomeNullableDateTime = 1.January(2001).AsUtc(),
            SomeNullableDateTimeOffset = 1.January(2001).AsUtc(),
            SomeNullableTimeSpan = TimeSpan.FromHours(1),
            SomeNullableEnum = DayOfWeek.Friday
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string attributeName = propertyName.Camelize();
        string route = $"/filterableResources?filter=not(equals({attributeName},null))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey(attributeName).With(value => value.Should().NotBeNull());
    }
}
