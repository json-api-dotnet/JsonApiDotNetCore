using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Links
{
    public sealed class AtomicRelativeLinksWithNamespaceTests
        : IClassFixture<ExampleIntegrationTestContext<RelativeLinksInApiNamespaceStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly ExampleIntegrationTestContext<RelativeLinksInApiNamespaceStartup<OperationsDbContext>, OperationsDbContext> _testContext;

        public AtomicRelativeLinksWithNamespaceTests(
            ExampleIntegrationTestContext<RelativeLinksInApiNamespaceStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddControllersFromExampleProject();

                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });
        }

        [Fact]
        public async Task Create_resource_with_side_effects_returns_relative_links()
        {
            // Arrange
            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "textLanguages",
                            attributes = new
                            {
                            }
                        }
                    },
                    new
                    {
                        op = "add",
                        data = new
                        {
                            type = "recordCompanies",
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            const string route = "/api/operations";

            // Act
            (HttpResponseMessage httpResponse, AtomicOperationsDocument responseDocument) =
                await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();

            string languageLink = "/api/textLanguages/" + Guid.Parse(responseDocument.Results[0].SingleData.Id);

            responseDocument.Results[0].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Links.Self.Should().Be(languageLink);
            responseDocument.Results[0].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Self.Should().Be(languageLink + "/relationships/lyrics");
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Related.Should().Be(languageLink + "/lyrics");

            responseDocument.Results[1].SingleData.Should().NotBeNull();

            string companyLink = "/api/recordCompanies/" + short.Parse(responseDocument.Results[1].SingleData.Id);

            responseDocument.Results[1].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Links.Self.Should().Be(companyLink);
            responseDocument.Results[1].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Self.Should().Be(companyLink + "/relationships/tracks");
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Related.Should().Be(companyLink + "/tracks");
        }
    }
}
