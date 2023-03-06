using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

public sealed class OptimisticConcurrencyResourceTests : IClassFixture<IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> _testContext;
    private readonly ConcurrencyFakers _fakers = new();

    public OptimisticConcurrencyResourceTests(IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WebPagesController>();
        testContext.UseController<FriendlyUrlsController>();
        testContext.UseController<TextBlocksController>();
        testContext.UseController<ParagraphsController>();
        testContext.UseController<WebImagesController>();
        testContext.UseController<PageFootersController>();
        testContext.UseController<WebLinksController>();
    }

    [Fact]
    public async Task Includes_version_in_get_resources_response()
    {
        // Arrange
        Paragraph paragraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Paragraph>();
            dbContext.Paragraphs.Add(paragraph);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("paragraphs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(paragraph.StringId);
        responseDocument.Data.ManyValue[0].Version.Should().Be(paragraph.Version);
        responseDocument.Data.ManyValue[0].Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());
    }

    [Fact]
    public async Task Includes_version_in_get_resource_response()
    {
        // Arrange
        Paragraph paragraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(paragraph);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/paragraphs/{paragraph.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("paragraphs");
        responseDocument.Data.SingleValue.Id.Should().Be(paragraph.StringId);
        responseDocument.Data.SingleValue.Version.Should().Be(paragraph.Version);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());
    }

    [Fact]
    public async Task Includes_version_in_get_resource_response_with_sparse_fieldset()
    {
        // Arrange
        Paragraph paragraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(paragraph);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/paragraphs/{paragraph.StringId}?fields[paragraphs]=text";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("paragraphs");
        responseDocument.Data.SingleValue.Id.Should().Be(paragraph.StringId);
        responseDocument.Data.SingleValue.Version.Should().Be(paragraph.Version);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());
    }

    [Fact]
    public async Task Includes_version_in_get_secondary_resources_response()
    {
        // Arrange
        TextBlock block = _fakers.TextBlock.Generate();
        block.Paragraphs = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(block);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/textBlocks/{block.StringId}/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("paragraphs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(block.Paragraphs[0].StringId);
        responseDocument.Data.ManyValue[0].Version.Should().Be(block.Paragraphs[0].Version);
        responseDocument.Data.ManyValue[0].Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());
    }

    [Fact]
    public async Task Includes_version_in_get_secondary_resource_response()
    {
        // Arrange
        WebPage page = _fakers.WebPage.Generate();
        page.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(page);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webPages/{page.StringId}/url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("friendlyUrls");
        responseDocument.Data.SingleValue.Id.Should().Be(page.Url.StringId);
        responseDocument.Data.SingleValue.Version.Should().Be(page.Url.Version);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());
    }

    [Fact]
    public async Task Includes_version_in_get_ToMany_relationship_response()
    {
        // Arrange
        TextBlock block = _fakers.TextBlock.Generate();
        block.Paragraphs = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(block);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/textBlocks/{block.StringId}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("paragraphs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(block.Paragraphs[0].StringId);
        responseDocument.Data.ManyValue[0].Version.Should().Be(block.Paragraphs[0].Version);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().NotBeVersioned();
        responseDocument.Links.Related.Should().NotBeVersioned();
    }

    [Fact]
    public async Task Includes_version_in_get_ToOne_relationship_response()
    {
        // Arrange
        WebPage page = _fakers.WebPage.Generate();
        page.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(page);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/webPages/{page.StringId}/relationships/url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("friendlyUrls");
        responseDocument.Data.SingleValue.Id.Should().Be(page.Url.StringId);
        responseDocument.Data.SingleValue.Version.Should().Be(page.Url.Version);

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().NotBeVersioned();
        responseDocument.Links.Related.Should().NotBeVersioned();
    }

    [Fact]
    public async Task Ignores_incoming_version_in_get_resource_request()
    {
        // Arrange
        Paragraph paragraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(paragraph);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/paragraphs/{paragraph.StringId};v~{Unknown.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("paragraphs");
        responseDocument.Data.SingleValue.Id.Should().Be(paragraph.StringId);
        responseDocument.Data.SingleValue.Version.Should().Be(paragraph.Version);
    }

    [Fact]
    public async Task Fails_on_incoming_version_in_get_secondary_request()
    {
        // Arrange
        TextBlock block = _fakers.TextBlock.Generate();
        block.Paragraphs = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(block);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/textBlocks/{block.StringId};v~{Unknown.Version}/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'textBlocks' with ID '{block.StringId};v~{Unknown.Version}' does not exist.");
    }

    [Fact]
    public async Task Ignores_incoming_version_in_get_relationship_request()
    {
        // Arrange
        TextBlock block = _fakers.TextBlock.Generate();
        block.Paragraphs = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(block);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/textBlocks/{block.StringId};v~{Unknown.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Type.Should().Be("paragraphs");
        responseDocument.Data.ManyValue[0].Id.Should().Be(block.Paragraphs[0].StringId);
        responseDocument.Data.ManyValue[0].Version.Should().Be(block.Paragraphs[0].Version);
    }

    [Fact]
    public async Task Can_create_versioned_resource()
    {
        // Arrange
        string newText = _fakers.Paragraph.Generate().Text;

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                attributes = new
                {
                    text = newText
                }
            }
        };

        const string route = "/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse.Headers.Location.ShouldNotBeNull();
        httpResponse.Headers.Location.ToString().Should().BeVersioned();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("paragraphs");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(0);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        long newParagraphId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Paragraph paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdAsync(newParagraphId);

            paragraphInDatabase.Text.Should().Be(newText);
            paragraphInDatabase.Version.Should().Be(responseDocument.Data.SingleValue.Version);
        });
    }

    [Fact]
    public async Task Can_create_versioned_resource_with_OneToOne_relationship_at_principal_side()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        string newUri = _fakers.FriendlyUrl.Generate().Uri;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                attributes = new
                {
                    uri = newUri
                },
                relationships = new
                {
                    page = new
                    {
                        data = new
                        {
                            type = "webPages",
                            id = existingPage.StringId,
                            version = existingPage.Version
                        }
                    }
                }
            }
        };

        const string route = "/friendlyUrls?include=page";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("friendlyUrls");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(0);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("page").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();

            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Type.Should().Be("webPages");
            value.Data.SingleValue.Id.Should().Be(existingPage.StringId);
            value.Data.SingleValue.Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("webPages");
        responseDocument.Included[0].Id.Should().Be(existingPage.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);

        responseDocument.Included[0].Relationships.ShouldContainKey("footer").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();
        });

        long newUrlId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FriendlyUrl urlInDatabase = await dbContext.FriendlyUrls.Include(url => url.Page).FirstWithIdAsync(newUrlId);

            urlInDatabase.Uri.Should().Be(newUri);
            urlInDatabase.Version.Should().Be(responseDocument.Data.SingleValue.Version);
            urlInDatabase.Page.ShouldNotBeNull();
            urlInDatabase.Page.Id.Should().Be(existingPage.Id);
            urlInDatabase.Page.ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_create_versioned_resource_with_OneToOne_relationship_at_principal_side_having_stale_token()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        string newUri = _fakers.FriendlyUrl.Generate().Uri;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                attributes = new
                {
                    uri = newUri
                },
                relationships = new
                {
                    page = new
                    {
                        data = new
                        {
                            type = "webPages",
                            id = existingPage.StringId,
                            version = existingPage.Version
                        }
                    }
                }
            }
        };

        const string route = "/friendlyUrls";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_versioned_resource_with_OneToOne_relationship_at_dependent_side()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        string newTitle = _fakers.WebPage.Generate().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                attributes = new
                {
                    title = newTitle
                },
                relationships = new
                {
                    url = new
                    {
                        data = new
                        {
                            type = "friendlyUrls",
                            id = existingUrl.StringId,
                            version = existingUrl.Version
                        }
                    }
                }
            }
        };

        const string route = "/webPages?include=url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("webPages");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(0);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("friendlyUrls");
        responseDocument.Included[0].Id.Should().Be(existingUrl.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingUrl.ConcurrencyToken);

        long newPageId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebPage pageInDatabase = await dbContext.WebPages.Include(page => page.Url).FirstWithIdAsync(newPageId);

            pageInDatabase.Title.Should().Be(newTitle);
            pageInDatabase.Version.Should().Be(responseDocument.Data.SingleValue.Version);
            pageInDatabase.Url.ShouldNotBeNull();
            pageInDatabase.Url.Id.Should().Be(existingUrl.Id);
            pageInDatabase.Url.ConcurrencyToken.Should().BeGreaterThan(existingUrl.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_create_versioned_resource_with_OneToOne_relationship_at_dependent_side_having_stale_token()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        string newTitle = _fakers.WebPage.Generate().Title;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"FriendlyUrls\" set \"Uri\" = 'other' where \"Id\" = {existingUrl.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                attributes = new
                {
                    title = newTitle
                },
                relationships = new
                {
                    url = new
                    {
                        data = new
                        {
                            type = "friendlyUrls",
                            id = existingUrl.StringId,
                            version = existingUrl.Version
                        }
                    }
                }
            }
        };

        const string route = "/webPages";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_versioned_resource_with_OneToMany_relationship()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        string newCopyright = _fakers.PageFooter.Generate().Copyright!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                attributes = new
                {
                    copyright = newCopyright
                },
                relationships = new
                {
                    usedAt = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webPages",
                                id = existingPage.StringId,
                                version = existingPage.Version
                            }
                        }
                    }
                }
            }
        };

        const string route = "/pageFooters?include=usedAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("pageFooters");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(0);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("usedAt").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();

            value.Data.ManyValue.ShouldHaveCount(1);
            value.Data.ManyValue[0].Type.Should().Be("webPages");
            value.Data.ManyValue[0].Id.Should().Be(existingPage.StringId);
            value.Data.ManyValue[0].Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("webPages");
        responseDocument.Included[0].Id.Should().Be(existingPage.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);

        responseDocument.Included[0].Relationships.ShouldContainKey("content").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();
        });

        long newFooterId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PageFooter footerInDatabase = await dbContext.PageFooters.Include(footer => footer.UsedAt).FirstWithIdAsync(newFooterId);

            footerInDatabase.Copyright.Should().Be(newCopyright);
            footerInDatabase.Version.Should().Be(responseDocument.Data.SingleValue.Version);
            footerInDatabase.UsedAt.ShouldHaveCount(1);
            footerInDatabase.UsedAt[0].Id.Should().Be(existingPage.Id);
            footerInDatabase.UsedAt[0].ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_create_versioned_resource_with_OneToMany_relationship_having_stale_token()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        string newCopyright = _fakers.PageFooter.Generate().Copyright!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                attributes = new
                {
                    copyright = newCopyright
                },
                relationships = new
                {
                    usedAt = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webPages",
                                id = existingPage.StringId,
                                version = existingPage.Version
                            }
                        }
                    }
                }
            }
        };

        const string route = "/pageFooters";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_versioned_resource_with_ManyToMany_relationship()
    {
        // Arrange
        WebLink existingLink = _fakers.WebLink.Generate();

        string newCopyright = _fakers.PageFooter.Generate().Copyright!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebLinks.Add(existingLink);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                attributes = new
                {
                    copyright = newCopyright
                },
                relationships = new
                {
                    links = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webLinks",
                                id = existingLink.StringId,
                                version = existingLink.Version
                            }
                        }
                    }
                }
            }
        };

        const string route = "/pageFooters?include=links";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("pageFooters");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(0);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("webLinks");
        responseDocument.Included[0].Id.Should().Be(existingLink.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingLink.ConcurrencyToken);

        long newFooterId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PageFooter footerInDatabase = await dbContext.PageFooters.Include(footer => footer.Links).FirstWithIdAsync(newFooterId);

            footerInDatabase.Copyright.Should().Be(newCopyright);
            footerInDatabase.Version.Should().Be(responseDocument.Data.SingleValue.Version);
            footerInDatabase.Links.ShouldHaveCount(1);
            footerInDatabase.Links[0].Id.Should().Be(existingLink.Id);
            footerInDatabase.Links[0].ConcurrencyToken.Should().BeGreaterThan(existingLink.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_create_versioned_resource_with_ManyToMany_relationship_having_stale_token()
    {
        // Arrange
        WebLink existingLink = _fakers.WebLink.Generate();

        string newCopyright = _fakers.PageFooter.Generate().Copyright!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebLinks.Add(existingLink);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebLinks\" set \"Text\" = 'other' where \"Id\" = {existingLink.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                attributes = new
                {
                    copyright = newCopyright
                },
                relationships = new
                {
                    links = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webLinks",
                                id = existingLink.StringId,
                                version = existingLink.Version
                            }
                        }
                    }
                }
            }
        };

        const string route = "/pageFooters";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_update_versioned_resource()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        string newText = _fakers.Paragraph.Generate().Text;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = existingParagraph.StringId,
                version = existingParagraph.Version,
                attributes = new
                {
                    text = newText
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("paragraphs");
        responseDocument.Data.SingleValue.Id.Should().Be(existingParagraph.StringId);
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(existingParagraph.ConcurrencyToken);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Paragraph paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdAsync(existingParagraph.Id);

            paragraphInDatabase.Text.Should().Be(newText);
            paragraphInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingParagraph.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Can_update_versioned_resource_without_changes()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = existingParagraph.StringId,
                version = existingParagraph.Version
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Paragraph paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdAsync(existingParagraph.Id);

            paragraphInDatabase.ConcurrencyToken.Should().Be(existingParagraph.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_having_stale_token()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        string newText = _fakers.Paragraph.Generate().Text;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingParagraph.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = existingParagraph.StringId,
                version = existingParagraph.Version,
                attributes = new
                {
                    text = newText
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_unknown_versioned_resource()
    {
        // Arrange
        string paragraphId = Unknown.StringId.For<Paragraph, long>();
        const string paragraphVersion = Unknown.Version;

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = paragraphId,
                version = paragraphVersion
            }
        };

        string route = $"/paragraphs/{paragraphId};v~{paragraphVersion}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'paragraphs' with ID '{paragraphId}' does not exist.");
    }

    [Fact]
    public async Task Can_update_versioned_resource_with_OneToOne_relationship_at_principal_side()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // Ensure assigned versions are different by saving in separate transactions.
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                id = existingUrl.StringId,
                version = existingUrl.Version,
                relationships = new
                {
                    page = new
                    {
                        data = new
                        {
                            type = "webPages",
                            id = existingPage.StringId,
                            version = existingPage.Version
                        }
                    }
                }
            }
        };

        string route = $"/friendlyUrls/{existingUrl.StringId};v~{existingUrl.Version}?include=page";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(existingUrl.StringId);
        responseDocument.Data.SingleValue.Type.Should().Be("friendlyUrls");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(existingUrl.ConcurrencyToken);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("page").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();

            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Type.Should().Be("webPages");
            value.Data.SingleValue.Id.Should().Be(existingPage.StringId);
            value.Data.SingleValue.Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("webPages");
        responseDocument.Included[0].Id.Should().Be(existingPage.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);

        responseDocument.Included[0].Relationships.ShouldContainKey("footer").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FriendlyUrl urlInDatabase = await dbContext.FriendlyUrls.Include(url => url.Page).FirstWithIdAsync(existingUrl.Id);

            urlInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingUrl.ConcurrencyToken);
            urlInDatabase.Page.ShouldNotBeNull();
            urlInDatabase.Page.Id.Should().Be(existingPage.Id);
            urlInDatabase.Page.ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_OneToOne_relationship_at_principal_side_having_stale_left_token()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"FriendlyUrls\" set \"Uri\" = 'other' where \"Id\" = {existingUrl.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                id = existingUrl.StringId,
                version = existingUrl.Version,
                relationships = new
                {
                    page = new
                    {
                        data = new
                        {
                            type = "webPages",
                            id = existingPage.StringId,
                            version = existingPage.Version
                        }
                    }
                }
            }
        };

        string route = $"/friendlyUrls/{existingUrl.StringId};v~{existingUrl.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_OneToOne_relationship_at_principal_side_having_stale_right_token()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                id = existingUrl.StringId,
                version = existingUrl.Version,
                relationships = new
                {
                    page = new
                    {
                        data = new
                        {
                            type = "webPages",
                            id = existingPage.StringId,
                            version = existingPage.Version
                        }
                    }
                }
            }
        };

        string route = $"/friendlyUrls/{existingUrl.StringId};v~{existingUrl.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_update_versioned_resource_with_OneToOne_relationship_at_dependent_side()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                id = existingPage.StringId,
                version = existingPage.Version,
                relationships = new
                {
                    url = new
                    {
                        data = new
                        {
                            type = "friendlyUrls",
                            id = existingUrl.StringId,
                            version = existingUrl.Version
                        }
                    }
                }
            }
        };

        string route = $"/webPages/{existingPage.StringId};v~{existingPage.Version}?include=url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(existingPage.StringId);
        responseDocument.Data.SingleValue.Type.Should().Be("webPages");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("friendlyUrls");
        responseDocument.Included[0].Id.Should().Be(existingUrl.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingUrl.ConcurrencyToken);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebPage pageInDatabase = await dbContext.WebPages.Include(page => page.Url).FirstWithIdAsync(existingPage.Id);

            pageInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
            pageInDatabase.Url.ShouldNotBeNull();
            pageInDatabase.Url.Id.Should().Be(existingUrl.Id);
            pageInDatabase.Url.ConcurrencyToken.Should().BeGreaterThan(existingUrl.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_OneToOne_relationship_at_dependent_side_having_stale_left_token()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                id = existingPage.StringId,
                version = existingPage.Version,
                relationships = new
                {
                    url = new
                    {
                        data = new
                        {
                            type = "friendlyUrls",
                            id = existingUrl.StringId,
                            version = existingUrl.Version
                        }
                    }
                }
            }
        };

        string route = $"/webPages/{existingPage.StringId};v~{existingPage.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_OneToOne_relationship_at_dependent_side_having_stale_right_token()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"FriendlyUrls\" set \"Uri\" = 'other' where \"Id\" = {existingUrl.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                id = existingPage.StringId,
                version = existingPage.Version,
                relationships = new
                {
                    url = new
                    {
                        data = new
                        {
                            type = "friendlyUrls",
                            id = existingUrl.StringId,
                            version = existingUrl.Version
                        }
                    }
                }
            }
        };

        string route = $"/webPages/{existingPage.StringId};v~{existingPage.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_update_versioned_resource_with_OneToMany_relationship()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PageFooters.Add(existingFooter);
            await dbContext.SaveChangesAsync();
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                id = existingFooter.StringId,
                version = existingFooter.Version,
                relationships = new
                {
                    usedAt = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webPages",
                                id = existingPage.StringId,
                                version = existingPage.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}?include=usedAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(existingFooter.StringId);
        responseDocument.Data.SingleValue.Type.Should().Be("pageFooters");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(existingFooter.ConcurrencyToken);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("usedAt").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();

            value.Data.ManyValue.ShouldHaveCount(1);
            value.Data.ManyValue[0].Type.Should().Be("webPages");
            value.Data.ManyValue[0].Id.Should().Be(existingPage.StringId);
            value.Data.ManyValue[0].Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("webPages");
        responseDocument.Included[0].Id.Should().Be(existingPage.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingPage.ConcurrencyToken);

        responseDocument.Included[0].Relationships.ShouldContainKey("content").With(value =>
        {
            value.ShouldNotBeNull();
            value.Links.ShouldNotBeNull();
            value.Links.Self.Should().BeVersioned();
            value.Links.Related.Should().NotBeVersioned();
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PageFooter footerInDatabase = await dbContext.PageFooters.Include(footer => footer.UsedAt).FirstWithIdAsync(existingFooter.Id);

            footerInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingFooter.ConcurrencyToken);
            footerInDatabase.UsedAt.ShouldHaveCount(1);
            footerInDatabase.UsedAt[0].Id.Should().Be(existingPage.Id);
            footerInDatabase.UsedAt[0].ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_OneToMany_relationship_having_stale_left_token()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PageFooters.Add(existingFooter);
            await dbContext.SaveChangesAsync();
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"PageFooters\" set \"Copyright\" = 'other' where \"Id\" = {existingFooter.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                id = existingFooter.StringId,
                version = existingFooter.Version,
                relationships = new
                {
                    usedAt = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webPages",
                                id = existingPage.StringId,
                                version = existingPage.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_OneToMany_relationship_having_stale_right_token()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PageFooters.Add(existingFooter);
            await dbContext.SaveChangesAsync();
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                id = existingFooter.StringId,
                version = existingFooter.Version,
                relationships = new
                {
                    usedAt = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webPages",
                                id = existingPage.StringId,
                                version = existingPage.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_update_versioned_resource_with_ManyToMany_relationship()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebLink existingLink = _fakers.WebLink.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PageFooters.Add(existingFooter);
            await dbContext.SaveChangesAsync();
            dbContext.WebLinks.Add(existingLink);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                id = existingFooter.StringId,
                version = existingFooter.Version,
                relationships = new
                {
                    links = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webLinks",
                                id = existingLink.StringId,
                                version = existingLink.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}?include=links";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(existingFooter.StringId);
        responseDocument.Data.SingleValue.Type.Should().Be("pageFooters");
        responseDocument.Data.SingleValue.Version.Should().BeGreaterThan(existingFooter.ConcurrencyToken);
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull().With(value => value.Self.Should().BeVersioned());

        responseDocument.Included.ShouldHaveCount(1);

        responseDocument.Included[0].Type.Should().Be("webLinks");
        responseDocument.Included[0].Id.Should().Be(existingLink.StringId);
        responseDocument.Included[0].Version.Should().BeGreaterThan(existingLink.ConcurrencyToken);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PageFooter footerInDatabase = await dbContext.PageFooters.Include(footer => footer.Links).FirstWithIdAsync(existingFooter.Id);

            footerInDatabase.ConcurrencyToken.Should().BeGreaterOrEqualTo(existingFooter.ConcurrencyToken);
            footerInDatabase.Links.ShouldHaveCount(1);
            footerInDatabase.Links[0].Id.Should().Be(existingLink.Id);
            footerInDatabase.Links[0].ConcurrencyToken.Should().BeGreaterThan(existingLink.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_ManyToMany_relationship_having_stale_left_token()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebLink existingLink = _fakers.WebLink.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PageFooters.Add(existingFooter);
            await dbContext.SaveChangesAsync();
            dbContext.WebLinks.Add(existingLink);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"PageFooters\" set \"Copyright\" = 'other' where \"Id\" = {existingFooter.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                id = existingFooter.StringId,
                version = existingFooter.Version,
                relationships = new
                {
                    links = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webLinks",
                                id = existingLink.StringId,
                                version = existingLink.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_versioned_resource_with_ManyToMany_relationship_having_stale_right_token()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebLink existingLink = _fakers.WebLink.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.PageFooters.Add(existingFooter);
            await dbContext.SaveChangesAsync();
            dbContext.WebLinks.Add(existingLink);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebLinks\" set \"Text\" = 'other' where \"Id\" = {existingLink.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "pageFooters",
                id = existingFooter.StringId,
                version = existingFooter.Version,
                relationships = new
                {
                    links = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "webLinks",
                                id = existingLink.StringId,
                                version = existingLink.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_delete_versioned_resource()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Paragraph? paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdOrDefaultAsync(existingParagraph.Id);

            paragraphInDatabase.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_delete_versioned_resource_having_stale_token()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Paragraphs.Add(existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingParagraph.Id}");
        });

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_delete_unknown_versioned_resource()
    {
        // Arrange
        string paragraphId = Unknown.StringId.For<Paragraph, long>();
        const string paragraphVersion = Unknown.Version;

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = paragraphId,
                version = paragraphVersion
            }
        };

        string route = $"/paragraphs/{paragraphId};v~{paragraphVersion}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'paragraphs' with ID '{paragraphId}' does not exist.");
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_at_principal_side_on_versioned_resource()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingUrl, existingPage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                id = existingPage.StringId,
                version = existingPage.Version
            }
        };

        string route = $"/friendlyUrls/{existingUrl.StringId};v~{existingUrl.Version}/relationships/page";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            FriendlyUrl urlInDatabase = await dbContext.FriendlyUrls.Include(url => url.Page).FirstWithIdAsync(existingUrl.Id);

            urlInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingUrl.ConcurrencyToken);
            urlInDatabase.Page.ShouldNotBeNull();
            urlInDatabase.Page.Id.Should().Be(existingPage.Id);
            urlInDatabase.Page.ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_replace_OneToOne_relationship_at_principal_side_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingUrl, existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"FriendlyUrls\" set \"Uri\" = 'other' where \"Id\" = {existingUrl.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                id = existingPage.StringId,
                version = existingPage.Version
            }
        };

        string route = $"/friendlyUrls/{existingUrl.StringId};v~{existingUrl.Version}/relationships/page";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_replace_OneToOne_relationship_at_principal_side_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingUrl, existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "webPages",
                id = existingPage.StringId,
                version = existingPage.Version
            }
        };

        string route = $"/friendlyUrls/{existingUrl.StringId};v~{existingUrl.Version}/relationships/page";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_replace_OneToOne_relationship_at_dependent_side_on_versioned_resource()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPage, existingUrl);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                id = existingUrl.StringId,
                version = existingUrl.Version
            }
        };

        string route = $"/webPages/{existingPage.StringId};v~{existingPage.Version}/relationships/url";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebPage pageInDatabase = await dbContext.WebPages.Include(page => page.Url).FirstWithIdAsync(existingPage.Id);

            pageInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
            pageInDatabase.Url.ShouldNotBeNull();
            pageInDatabase.Url.Id.Should().Be(existingUrl.Id);
            pageInDatabase.Url.ConcurrencyToken.Should().BeGreaterThan(existingUrl.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_replace_OneToOne_relationship_at_dependent_side_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPage, existingUrl);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                id = existingUrl.StringId,
                version = existingUrl.Version
            }
        };

        string route = $"/webPages/{existingPage.StringId};v~{existingPage.Version}/relationships/url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_replace_OneToOne_relationship_at_dependent_side_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingPage, existingUrl);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"FriendlyUrls\" set \"Uri\" = 'other' where \"Id\" = {existingUrl.Id}");
        });

        var requestBody = new
        {
            data = new
            {
                type = "friendlyUrls",
                id = existingUrl.StringId,
                version = existingUrl.Version
            }
        };

        string route = $"/webPages/{existingPage.StringId};v~{existingPage.Version}/relationships/url";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_replace_OneToMany_relationship_on_versioned_resource()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingFooter, existingPage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "webPages",
                    id = existingPage.StringId,
                    version = existingPage.Version
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}/relationships/usedAt";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            PageFooter footerInDatabase = await dbContext.PageFooters.Include(footer => footer.UsedAt).FirstWithIdAsync(existingFooter.Id);

            footerInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingFooter.ConcurrencyToken);
            footerInDatabase.UsedAt.ShouldHaveCount(1);
            footerInDatabase.UsedAt[0].Id.Should().Be(existingPage.Id);
            footerInDatabase.UsedAt[0].ConcurrencyToken.Should().BeGreaterThan(existingPage.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_replace_OneToMany_relationship_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingFooter, existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"PageFooters\" set \"Copyright\" = 'other' where \"Id\" = {existingFooter.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "webPages",
                    id = existingPage.StringId,
                    version = existingPage.Version
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}/relationships/usedAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_replace_OneToMany_relationship_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        PageFooter existingFooter = _fakers.PageFooter.Generate();

        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingFooter, existingPage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebPages\" set \"Title\" = 'other' where \"Id\" = {existingPage.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "webPages",
                    id = existingPage.StringId,
                    version = existingPage.Version
                }
            }
        };

        string route = $"/pageFooters/{existingFooter.StringId};v~{existingFooter.Version}/relationships/usedAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_replace_ManyToMany_relationship_on_versioned_resource()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBlock, existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextBlock blockInDatabase = await dbContext.TextBlocks.Include(block => block.Paragraphs).FirstWithIdAsync(existingBlock.Id);

            blockInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingBlock.ConcurrencyToken);
            blockInDatabase.Paragraphs.ShouldHaveCount(1);
            blockInDatabase.Paragraphs[0].Id.Should().Be(existingParagraph.Id);
            blockInDatabase.Paragraphs[0].ConcurrencyToken.Should().BeGreaterThan(existingParagraph.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_replace_ManyToMany_relationship_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBlock, existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"TextBlocks\" set \"ColumnCount\" = 0 where \"Id\" = {existingBlock.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_replace_ManyToMany_relationship_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBlock, existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingParagraph.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_replace_relationship_for_unknown_versioned_resource()
    {
        // Arrange
        string unknownFooterId = Unknown.StringId.For<PageFooter, long>();

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "webPages",
                    id = unknownFooterId,
                    version = Unknown.Version
                }
            }
        };

        string route = $"/pageFooters/{unknownFooterId};v~{Unknown.Version}/relationships/usedAt";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().StartWith("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'pageFooters' with ID '{unknownFooterId}' does not exist.");
    }

    [Fact]
    public async Task Can_add_to_OneToMany_relationship_on_versioned_resource()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingImage, existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebImage imageInDatabase = await dbContext.WebImages.Include(image => image.UsedIn).FirstWithIdAsync(existingImage.Id);

            imageInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingImage.ConcurrencyToken);
            imageInDatabase.UsedIn.ShouldHaveCount(2);

            Paragraph paragraphInDatabase = imageInDatabase.UsedIn.Single(paragraph => paragraph.Id == existingParagraph.Id);
            paragraphInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingParagraph.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Can_add_to_OneToMany_relationship_on_versioned_resource_without_changes()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebImages.Add(existingImage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingImage.UsedIn[0].StringId,
                    version = existingImage.UsedIn[0].Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebImage imageInDatabase = await dbContext.WebImages.Include(image => image.UsedIn).FirstWithIdAsync(existingImage.Id);

            imageInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingImage.ConcurrencyToken);
            imageInDatabase.UsedIn.ShouldHaveCount(1);
            imageInDatabase.UsedIn[0].Id.Should().Be(existingImage.UsedIn[0].Id);
            imageInDatabase.UsedIn[0].ConcurrencyToken.Should().BeGreaterThan(existingImage.UsedIn[0].ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_add_to_OneToMany_relationship_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingImage, existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebImages\" set \"Description\" = 'other' where \"Id\" = {existingImage.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_to_OneToMany_relationship_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingImage, existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingParagraph.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_add_to_ManyToMany_relationship_on_versioned_resource()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBlock, existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextBlock blockInDatabase = await dbContext.TextBlocks.Include(block => block.Paragraphs).FirstWithIdAsync(existingBlock.Id);

            blockInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingBlock.ConcurrencyToken);
            blockInDatabase.Paragraphs.ShouldHaveCount(2);

            Paragraph paragraphInDatabase = blockInDatabase.Paragraphs.Single(paragraph => paragraph.Id == existingParagraph.Id);
            paragraphInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingParagraph.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Can_add_to_ManyToMany_relationship_on_versioned_resource_without_changes()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingBlock.Paragraphs[0].StringId,
                    version = existingBlock.Paragraphs[0].Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextBlock blockInDatabase = await dbContext.TextBlocks.Include(block => block.Paragraphs).FirstWithIdAsync(existingBlock.Id);

            blockInDatabase.ConcurrencyToken.Should().Be(existingBlock.ConcurrencyToken);
            blockInDatabase.Paragraphs.ShouldHaveCount(1);
            blockInDatabase.Paragraphs[0].Id.Should().Be(existingBlock.Paragraphs[0].Id);
            blockInDatabase.Paragraphs[0].ConcurrencyToken.Should().Be(existingBlock.Paragraphs[0].ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_add_to_ManyToMany_relationship_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBlock, existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"TextBlocks\" set \"ColumnCount\" = 0 where \"Id\" = {existingBlock.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_to_ManyToMany_relationship_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingBlock, existingParagraph);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingParagraph.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_to_relationship_for_unknown_versioned_resource()
    {
        // Arrange
        string unknownImageId = Unknown.StringId.For<WebImage, long>();

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = Unknown.StringId.For<Paragraph, long>(),
                    version = Unknown.Version
                }
            }
        };

        string route = $"/webImages/{unknownImageId};v~{Unknown.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().StartWith("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'webImages' with ID '{unknownImageId}' does not exist.");
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship_on_versioned_resource()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebImages.Add(existingImage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingImage.UsedIn[0].StringId,
                    version = existingImage.UsedIn[0].Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebImage imageInDatabase = await dbContext.WebImages.Include(image => image.UsedIn).FirstWithIdAsync(existingImage.Id);

            imageInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingImage.ConcurrencyToken);
            imageInDatabase.UsedIn.ShouldHaveCount(1);
            imageInDatabase.UsedIn[0].Id.Should().Be(existingImage.UsedIn[1].Id);
            imageInDatabase.UsedIn[0].ConcurrencyToken.Should().Be(existingImage.UsedIn[1].ConcurrencyToken);

            Paragraph paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdAsync(existingImage.UsedIn[0].Id);
            paragraphInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingImage.UsedIn[0].ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Can_remove_from_OneToMany_relationship_on_versioned_resource_without_changes()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingImage, existingParagraph);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingParagraph.StringId,
                    version = existingParagraph.Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WebImage imageInDatabase = await dbContext.WebImages.Include(image => image.UsedIn).FirstWithIdAsync(existingImage.Id);

            imageInDatabase.ConcurrencyToken.Should().Be(existingImage.ConcurrencyToken);
            imageInDatabase.UsedIn.ShouldHaveCount(1);
            imageInDatabase.UsedIn[0].Id.Should().Be(existingImage.UsedIn[0].Id);
            imageInDatabase.UsedIn[0].ConcurrencyToken.Should().Be(existingImage.UsedIn[0].ConcurrencyToken);

            Paragraph paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdAsync(existingParagraph.Id);
            paragraphInDatabase.ConcurrencyToken.Should().Be(existingParagraph.ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_remove_from_OneToMany_relationship_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebImages.Add(existingImage);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"WebImages\" set \"Description\" = 'other' where \"Id\" = {existingImage.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingImage.UsedIn[0].StringId,
                    version = existingImage.UsedIn[0].Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_from_OneToMany_relationship_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();
        existingImage.UsedIn = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebImages.Add(existingImage);
            await dbContext.SaveChangesAsync();

            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingImage.UsedIn[0].Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingImage.UsedIn[0].StringId,
                    version = existingImage.UsedIn[0].Version
                }
            }
        };

        string route = $"/webImages/{existingImage.StringId};v~{existingImage.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Can_remove_from_ManyToMany_relationship_on_versioned_resource()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingBlock.Paragraphs[0].StringId,
                    version = existingBlock.Paragraphs[0].Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            TextBlock blockInDatabase = await dbContext.TextBlocks.Include(block => block.Paragraphs).FirstWithIdAsync(existingBlock.Id);

            blockInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingBlock.ConcurrencyToken);
            blockInDatabase.Paragraphs.ShouldHaveCount(1);
            blockInDatabase.Paragraphs[0].Id.Should().Be(existingBlock.Paragraphs[1].Id);
            blockInDatabase.Paragraphs[0].ConcurrencyToken.Should().Be(existingBlock.Paragraphs[1].ConcurrencyToken);

            Paragraph paragraphInDatabase = await dbContext.Paragraphs.FirstWithIdAsync(existingBlock.Paragraphs[0].Id);
            paragraphInDatabase.ConcurrencyToken.Should().BeGreaterThan(existingBlock.Paragraphs[0].ConcurrencyToken);
        });
    }

    [Fact]
    public async Task Cannot_remove_from_ManyToMany_relationship_on_versioned_resource_having_stale_left_token()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(existingBlock);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"update \"TextBlocks\" set \"ColumnCount\" = 0 where \"Id\" = {existingBlock.Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingBlock.Paragraphs[0].StringId,
                    version = existingBlock.Paragraphs[0].Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_from_ManyToMany_relationship_on_versioned_resource_having_stale_right_token()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();
        existingBlock.Paragraphs = _fakers.Paragraph.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(existingBlock);
            await dbContext.SaveChangesAsync();

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"update \"Paragraphs\" set \"Text\" = 'other' where \"Id\" = {existingBlock.Paragraphs[0].Id}");
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = existingBlock.Paragraphs[0].StringId,
                    version = existingBlock.Paragraphs[0].Version
                }
            }
        };

        string route = $"/textBlocks/{existingBlock.StringId};v~{existingBlock.Version}/relationships/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().StartWith("The resource version does not match the server version.");
        error.Detail.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_from_relationship_for_unknown_versioned_resource()
    {
        // Arrange
        string unknownImageId = Unknown.StringId.For<WebImage, long>();

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "paragraphs",
                    id = Unknown.StringId.For<Paragraph, long>(),
                    version = Unknown.Version
                }
            }
        };

        string route = $"/webImages/{unknownImageId};v~{Unknown.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().StartWith("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'webImages' with ID '{unknownImageId}' does not exist.");
    }
}
