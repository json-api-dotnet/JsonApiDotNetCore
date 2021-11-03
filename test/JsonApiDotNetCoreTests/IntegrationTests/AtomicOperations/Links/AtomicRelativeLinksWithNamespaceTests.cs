using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Links
{
    public sealed class AtomicRelativeLinksWithNamespaceTests
        : IClassFixture<IntegrationTestContext<RelativeLinksInApiNamespaceStartup<OperationsDbContext>, OperationsDbContext>>
    {
        private readonly IntegrationTestContext<RelativeLinksInApiNamespaceStartup<OperationsDbContext>, OperationsDbContext> _testContext;
        private readonly OperationsFakers _fakers = new();

        public AtomicRelativeLinksWithNamespaceTests(
            IntegrationTestContext<RelativeLinksInApiNamespaceStartup<OperationsDbContext>, OperationsDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<OperationsController>();

            // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
            testContext.UseController<TextLanguagesController>();
            testContext.UseController<RecordCompaniesController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
            });
        }

        [Fact]
        public async Task Create_resource_with_side_effects_returns_relative_links()
        {
            // Arrange
            string newCompanyName = _fakers.RecordCompany.Generate().Name;

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
                                name = newCompanyName
                            }
                        }
                    }
                }
            };

            const string route = "/api/operations";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Results.ShouldHaveCount(2);

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull();

            responseDocument.Results[0].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                string languageLink = $"/api/textLanguages/{Guid.Parse(resource.Id.ShouldNotBeNull())}";

                resource.Links.ShouldNotBeNull();
                resource.Links.Self.Should().Be(languageLink);

                resource.Relationships.ShouldContainKey("lyrics").With(value =>
                {
                    value.ShouldNotBeNull();
                    value.Links.ShouldNotBeNull();
                    value.Links.Self.Should().Be($"{languageLink}/relationships/lyrics");
                    value.Links.Related.Should().Be($"{languageLink}/lyrics");
                });
            });

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull();

            responseDocument.Results[1].Data.SingleValue.ShouldNotBeNull().With(resource =>
            {
                string companyLink = $"/api/recordCompanies/{short.Parse(resource.Id.ShouldNotBeNull())}";

                resource.Links.ShouldNotBeNull();
                resource.Links.Self.Should().Be(companyLink);

                resource.Relationships.ShouldContainKey("tracks").With(value =>
                {
                    value.ShouldNotBeNull();
                    value.Links.ShouldNotBeNull();
                    value.Links.Self.Should().Be($"{companyLink}/relationships/tracks");
                    value.Links.Related.Should().Be($"{companyLink}/tracks");
                });
            });
        }
    }
}
