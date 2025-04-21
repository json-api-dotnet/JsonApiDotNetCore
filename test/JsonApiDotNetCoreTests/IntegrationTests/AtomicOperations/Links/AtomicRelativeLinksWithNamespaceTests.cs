using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Links;

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

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Fact]
    public async Task Create_resource_with_side_effects_returns_relative_links()
    {
        // Arrange
        string newCompanyName = _fakers.RecordCompany.GenerateOne().Name;

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.Should().NotBeNull();

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            string languageLink = $"/api/textLanguages/{Guid.Parse(resource.Id.Should().NotBeNull().And.Subject)}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(languageLink);

            resource.Relationships.Should().ContainKey("lyrics").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{languageLink}/relationships/lyrics");
                value.Links.Related.Should().Be($"{languageLink}/lyrics");
            });
        });

        responseDocument.Results[1].Data.SingleValue.Should().NotBeNull();

        responseDocument.Results[1].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            string companyLink = $"/api/recordCompanies/{short.Parse(resource.Id.Should().NotBeNull().And.Subject)}";

            resource.Links.Should().NotBeNull();
            resource.Links.Self.Should().Be(companyLink);

            resource.Relationships.Should().ContainKey("tracks").WhoseValue.With(value =>
            {
                value.Should().NotBeNull();
                value.Links.Should().NotBeNull();
                value.Links.Self.Should().Be($"{companyLink}/relationships/tracks");
                value.Links.Related.Should().Be($"{companyLink}/tracks");
            });
        });
    }
}
