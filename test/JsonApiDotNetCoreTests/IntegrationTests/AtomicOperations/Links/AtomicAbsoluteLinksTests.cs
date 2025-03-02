using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Links;

public sealed class AtomicAbsoluteLinksTests : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private const string HostPrefix = "http://localhost";

    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicAbsoluteLinksTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        // These routes need to be registered in ASP.NET for rendering links to resource/relationship endpoints.
        testContext.UseController<TextLanguagesController>();
        testContext.UseController<RecordCompaniesController>();

        testContext.ConfigureServices(services => services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>)));
    }

    [Fact]
    public async Task Update_resource_with_side_effects_returns_absolute_links()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.GenerateOne();
        RecordCompany existingCompany = _fakers.RecordCompany.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingLanguage, existingCompany);
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

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(2);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            string languageLink = $"{HostPrefix}/textLanguages/{existingLanguage.StringId}";

            resource.Should().NotBeNull();
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

        responseDocument.Results[1].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            string companyLink = $"{HostPrefix}/recordCompanies/{existingCompany.StringId}";

            resource.Should().NotBeNull();
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

    [Fact]
    public async Task Update_resource_with_side_effects_and_missing_resource_controller_hides_links()
    {
        // Arrange
        Playlist existingPlaylist = _fakers.Playlist.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Playlists.Add(existingPlaylist);
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
                        type = "playlists",
                        id = existingPlaylist.StringId,
                        attributes = new
                        {
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Should().NotBeNull();
            resource.Links.Should().BeNull();
            resource.Relationships.Should().BeNull();
        });
    }
}
