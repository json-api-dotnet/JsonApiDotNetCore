using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    public sealed class ReadOnlyControllerTests : IClassFixture<IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> _testContext;
        private readonly RestrictionFakers _fakers = new();

        public ReadOnlyControllerTests(IntegrationTestContext<TestableStartup<RestrictionDbContext>, RestrictionDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<BedsController>();
        }

        [Fact]
        public async Task Can_get_resources()
        {
            // Arrange
            const string route = "/beds";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_get_resource()
        {
            // Arrange
            Bed bed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(bed);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/beds/{bed.StringId}";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_get_secondary_resources()
        {
            // Arrange
            Bed bed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(bed);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/beds/{bed.StringId}/pillows";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_get_secondary_resource()
        {
            // Arrange
            Bed bed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(bed);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/beds/{bed.StringId}/room";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Can_get_relationship()
        {
            // Arrange
            Bed bed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(bed);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/beds/{bed.StringId}/relationships/pillows";

            // Act
            (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Cannot_create_resource()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "beds",
                    attributes = new
                    {
                    }
                }
            };

            const string route = "/beds?include=pillows";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("The requested endpoint is not accessible.");
            error.Detail.Should().Be("Endpoint '/beds' is not accessible for POST requests.");
        }

        [Fact]
        public async Task Cannot_update_resource()
        {
            // Arrange
            Bed existingBed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(existingBed);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "beds",
                    id = existingBed.StringId,
                    attributes = new
                    {
                    }
                }
            };

            string route = $"/beds/{existingBed.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("The requested endpoint is not accessible.");
            error.Detail.Should().Be($"Endpoint '{route}' is not accessible for PATCH requests.");
        }

        [Fact]
        public async Task Cannot_delete_resource()
        {
            // Arrange
            Bed existingBed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(existingBed);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/beds/{existingBed.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("The requested endpoint is not accessible.");
            error.Detail.Should().Be($"Endpoint '{route}' is not accessible for DELETE requests.");
        }

        [Fact]
        public async Task Cannot_update_relationship()
        {
            // Arrange
            Bed existingBed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(existingBed);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = (object?)null
            };

            string route = $"/beds/{existingBed.StringId}/relationships/room";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("The requested endpoint is not accessible.");
            error.Detail.Should().Be($"Endpoint '{route}' is not accessible for PATCH requests.");
        }

        [Fact]
        public async Task Cannot_add_to_ToMany_relationship()
        {
            // Arrange
            Bed existingBed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(existingBed);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = Array.Empty<object>()
            };

            string route = $"/beds/{existingBed.StringId}/relationships/pillows";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("The requested endpoint is not accessible.");
            error.Detail.Should().Be($"Endpoint '{route}' is not accessible for POST requests.");
        }

        [Fact]
        public async Task Cannot_remove_from_ToMany_relationship()
        {
            // Arrange
            Bed existingBed = _fakers.Bed.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Beds.Add(existingBed);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = Array.Empty<object>()
            };

            string route = $"/beds/{existingBed.StringId}/relationships/pillows";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            error.Title.Should().Be("The requested endpoint is not accessible.");
            error.Detail.Should().Be($"Endpoint '{route}' is not accessible for DELETE requests.");
        }
    }
}
