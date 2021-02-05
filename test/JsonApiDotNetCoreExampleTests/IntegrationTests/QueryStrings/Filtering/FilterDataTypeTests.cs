using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Extensions;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Filtering
{
    public sealed class FilterDataTypeTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> _testContext;

        public FilterDataTypeTests(ExampleIntegrationTestContext<TestableStartup<FilterDbContext>, FilterDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
        }

        [Theory]
        [InlineData(nameof(FilterableResource.SomeString), "text")]
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
        public async Task Can_filter_equality_on_type(string propertyName, object value)
        {
            // Arrange
            var resource = new FilterableResource();
            var property = typeof(FilterableResource).GetProperty(propertyName);
            property?.SetValue(resource, value);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var attributeName = propertyName.Camelize();
            var route = $"/filterableResources?filter=equals({attributeName},'{value}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes[attributeName].Should().Be(value is Enum ? value.ToString() : value);
        }

        [Fact]
        public async Task Can_filter_equality_on_type_Decimal()
        {
            // Arrange
            var resource = new FilterableResource {SomeDecimal = 0.5m};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var route = $"/filterableResources?filter=equals(someDecimal,'{resource.SomeDecimal}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someDecimal"].Should().Be(resource.SomeDecimal);
        }

        [Fact]
        public async Task Can_filter_equality_on_type_Guid()
        {
            // Arrange
            var resource = new FilterableResource {SomeGuid = Guid.NewGuid()};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var route = $"/filterableResources?filter=equals(someGuid,'{resource.SomeGuid}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someGuid"].Should().Be(resource.SomeGuid.ToString());
        }

        [Fact]
        public async Task Can_filter_equality_on_type_DateTime()
        {
            // Arrange
            var resource = new FilterableResource {SomeDateTime = 27.January(2003).At(11, 22, 33, 44)};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var route = $"/filterableResources?filter=equals(someDateTime,'{resource.SomeDateTime:O}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someDateTime"].Should().BeCloseTo(resource.SomeDateTime);
        }

        [Fact]
        public async Task Can_filter_equality_on_type_DateTimeOffset()
        {
            // Arrange
            var resource = new FilterableResource
            {
                SomeDateTimeOffset = 27.January(2003).At(11, 22, 33, 44).ToDateTimeOffset(TimeSpan.FromHours(3))
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var route = $"/filterableResources?filter=equals(someDateTimeOffset,'{WebUtility.UrlEncode(resource.SomeDateTimeOffset.ToString("O"))}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someDateTimeOffset"].Should().BeCloseTo(resource.SomeDateTimeOffset);
        }

        [Fact]
        public async Task Can_filter_equality_on_type_TimeSpan()
        {
            // Arrange
            var resource = new FilterableResource {SomeTimeSpan = new TimeSpan(1, 2, 3, 4, 5)};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var route = $"/filterableResources?filter=equals(someTimeSpan,'{resource.SomeTimeSpan}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someTimeSpan"].Should().Be(resource.SomeTimeSpan.ToString());
        }

        [Fact]
        public async Task Cannot_filter_equality_on_incompatible_value()
        {
            // Arrange
            var resource = new FilterableResource {SomeInt32 = 1};

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var route = "/filterableResources?filter=equals(someInt32,'ABC')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Query creation failed due to incompatible types.");
            responseDocument.Errors[0].Detail.Should().Be("Failed to convert 'ABC' of type 'String' to type 'Int32'.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
        }

        [Theory]
        [InlineData(nameof(FilterableResource.SomeString))]
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
            var property = typeof(FilterableResource).GetProperty(propertyName);
            property?.SetValue(resource, null);

            var otherResource = new FilterableResource
            {
                SomeString = "X",
                SomeNullableBoolean = true,
                SomeNullableInt32 = 1,
                SomeNullableUnsignedInt64 = 1,
                SomeNullableDecimal = 1,
                SomeNullableDouble = 1,
                SomeNullableGuid = Guid.NewGuid(),
                SomeNullableDateTime = 1.January(2001),
                SomeNullableDateTimeOffset = 1.January(2001).ToDateTimeOffset(TimeSpan.FromHours(-1)),
                SomeNullableTimeSpan = TimeSpan.FromHours(1),
                SomeNullableEnum = DayOfWeek.Friday
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, otherResource);
                await dbContext.SaveChangesAsync();
            });

            var attributeName = propertyName.Camelize();
            var route = $"/filterableResources?filter=equals({attributeName},null)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes[attributeName].Should().Be(null);
        }

        [Theory]
        [InlineData(nameof(FilterableResource.SomeString))]
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
                SomeString = "X",
                SomeNullableBoolean = true,
                SomeNullableInt32 = 1,
                SomeNullableUnsignedInt64 = 1,
                SomeNullableDecimal = 1,
                SomeNullableDouble = 1,
                SomeNullableGuid = Guid.NewGuid(),
                SomeNullableDateTime = 1.January(2001),
                SomeNullableDateTimeOffset = 1.January(2001).ToDateTimeOffset(TimeSpan.FromHours(-1)),
                SomeNullableTimeSpan = TimeSpan.FromHours(1),
                SomeNullableEnum = DayOfWeek.Friday
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<FilterableResource>();
                dbContext.FilterableResources.AddRange(resource, new FilterableResource());
                await dbContext.SaveChangesAsync();
            });

            var attributeName = propertyName.Camelize();
            var route = $"/filterableResources?filter=not(equals({attributeName},null))";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes[attributeName].Should().NotBe(null);
        }
    }
}
