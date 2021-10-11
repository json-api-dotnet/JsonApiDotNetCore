#nullable disable

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers
{
    public sealed class NonJsonApiControllerTests : IClassFixture<IntegrationTestContext<TestableStartup<NonJsonApiDbContext>, NonJsonApiDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<NonJsonApiDbContext>, NonJsonApiDbContext> _testContext;

        public NonJsonApiControllerTests(IntegrationTestContext<TestableStartup<NonJsonApiDbContext>, NonJsonApiDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<NonJsonApiController>();
        }

        [Fact]
        public async Task Get_skips_middleware_and_formatters()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, "/NonJsonApi");

            HttpClient client = _testContext.Factory.CreateClient();

            // Act
            HttpResponseMessage httpResponse = await client.SendAsync(request);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should().Be("application/json; charset=utf-8");

            string responseText = await httpResponse.Content.ReadAsStringAsync();
            responseText.Should().Be("[\"Welcome!\"]");
        }

        [Fact]
        public async Task Post_skips_middleware_and_formatters()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Post, "/NonJsonApi")
            {
                Content = new StringContent("Jack")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("text/plain")
                    }
                }
            };

            HttpClient client = _testContext.Factory.CreateClient();

            // Act
            HttpResponseMessage httpResponse = await client.SendAsync(request);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should().Be("text/plain; charset=utf-8");

            string responseText = await httpResponse.Content.ReadAsStringAsync();
            responseText.Should().Be("Hello, Jack");
        }

        [Fact]
        public async Task Post_skips_error_handler()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Post, "/NonJsonApi");

            HttpClient client = _testContext.Factory.CreateClient();

            // Act
            HttpResponseMessage httpResponse = await client.SendAsync(request);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);
            httpResponse.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should().Be("text/plain; charset=utf-8");

            string responseText = await httpResponse.Content.ReadAsStringAsync();
            responseText.Should().Be("Please send your name.");
        }

        [Fact]
        public async Task Put_skips_middleware_and_formatters()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Put, "/NonJsonApi")
            {
                Content = new StringContent("\"Jane\"")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            HttpClient client = _testContext.Factory.CreateClient();

            // Act
            HttpResponseMessage httpResponse = await client.SendAsync(request);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should().Be("text/plain; charset=utf-8");

            string responseText = await httpResponse.Content.ReadAsStringAsync();
            responseText.Should().Be("Hi, Jane");
        }

        [Fact]
        public async Task Patch_skips_middleware_and_formatters()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Patch, "/NonJsonApi?name=Janice");

            HttpClient client = _testContext.Factory.CreateClient();

            // Act
            HttpResponseMessage httpResponse = await client.SendAsync(request);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should().Be("text/plain; charset=utf-8");

            string responseText = await httpResponse.Content.ReadAsStringAsync();
            responseText.Should().Be("Good day, Janice");
        }

        [Fact]
        public async Task Delete_skips_middleware_and_formatters()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Delete, "/NonJsonApi");

            HttpClient client = _testContext.Factory.CreateClient();

            // Act
            HttpResponseMessage httpResponse = await client.SendAsync(request);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            httpResponse.Content.Headers.ContentType.Should().NotBeNull().And.Subject.ToString().Should().Be("text/plain; charset=utf-8");

            string responseText = await httpResponse.Content.ReadAsStringAsync();
            responseText.Should().Be("Bye.");
        }
    }
}
