#nullable disable

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MultiDbContextExample;
using TestBuildingBlocks;
using Xunit;

namespace MultiDbContextTests
{
    public sealed class ResourceTests : IntegrationTest, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected override JsonSerializerOptions SerializerOptions
        {
            get
            {
                var options = _factory.Services.GetRequiredService<IJsonApiOptions>();
                return options.SerializerOptions;
            }
        }

        public ResourceTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Can_get_ResourceAs()
        {
            // Arrange
            const string route = "/resourceAs";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Attributes["nameA"].Should().Be("SampleA");
        }

        [Fact]
        public async Task Can_get_ResourceBs()
        {
            // Arrange
            const string route = "/resourceBs";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Attributes["nameB"].Should().Be("SampleB");
        }

        protected override HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }
    }
}
