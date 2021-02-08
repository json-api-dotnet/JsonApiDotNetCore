using System.Net;
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
    public sealed class AtomicAbsoluteLinksTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new OperationsFakers();

        public AtomicAbsoluteLinksTests(ExampleIntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddControllersFromExampleProject();

                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });
        }

        [Fact]
        public async Task Update_resource_with_side_effects_returns_absolute_links()
        {
            // Arrange
            var existingLanguage = _fakers.TextLanguage.Generate();
            var existingCompany = _fakers.RecordCompany.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingLanguage, existingCompany);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                atomic__operations = new object[]
                {
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "textLanguages",
                            id = existingLanguage.StringId,
                            attributes = new
                            {
                            }
                        }
                    },
                    new
                    {
                        op = "update",
                        data = new
                        {
                            type = "recordCompanies",
                            id = existingCompany.StringId,
                            attributes = new
                            {
                            }
                        }
                    }
                }
            };

            var route = "/operations";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAtomicAsync<AtomicOperationsDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.Should().HaveCount(2);

            responseDocument.Results[0].SingleData.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Links.Self.Should().Be("http://localhost/textLanguages/" + existingLanguage.StringId);
            responseDocument.Results[0].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Should().NotBeNull();
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Self.Should().Be($"http://localhost/textLanguages/{existingLanguage.StringId}/relationships/lyrics");
            responseDocument.Results[0].SingleData.Relationships["lyrics"].Links.Related.Should().Be($"http://localhost/textLanguages/{existingLanguage.StringId}/lyrics");

            responseDocument.Results[1].SingleData.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Links.Self.Should().Be("http://localhost/recordCompanies/" + existingCompany.StringId);
            responseDocument.Results[1].SingleData.Relationships.Should().NotBeEmpty();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Should().NotBeNull();
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Self.Should().Be($"http://localhost/recordCompanies/{existingCompany.StringId}/relationships/tracks");
            responseDocument.Results[1].SingleData.Relationships["tracks"].Links.Related.Should().Be($"http://localhost/recordCompanies/{existingCompany.StringId}/tracks");
        }
    }
}
