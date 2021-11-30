using System.Net;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

public sealed class OptimisticConcurrencyOperationsTests : IClassFixture<IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> _testContext;
    private readonly ConcurrencyFakers _fakers = new();

    public OptimisticConcurrencyOperationsTests(IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
        testContext.UseController<WebPagesController>();
        testContext.UseController<FriendlyUrlsController>();
        testContext.UseController<TextBlocksController>();
        testContext.UseController<ParagraphsController>();
        testContext.UseController<WebImagesController>();
        testContext.UseController<PageFootersController>();
        testContext.UseController<WebLinksController>();
    }

    [Fact(Skip = "Does not work, requires investigation.")]
    public async Task Tracks_versions_over_various_operations()
    {
        // Arrange
        WebPage existingPage = _fakers.WebPage.Generate();
        existingPage.Url = _fakers.FriendlyUrl.Generate();
        existingPage.Footer = _fakers.PageFooter.Generate();

        FriendlyUrl existingUrl = _fakers.FriendlyUrl.Generate();

        string newImagePath1 = _fakers.WebImage.Generate().Path;
        string newImagePath2 = _fakers.WebImage.Generate().Path;
        string newImageDescription = _fakers.WebImage.Generate().Description!;
        string newParagraphText = _fakers.Paragraph.Generate().Text;
        int newBlockColumnCount = _fakers.TextBlock.Generate().ColumnCount;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebPages.Add(existingPage);
            await dbContext.SaveChangesAsync();
            dbContext.FriendlyUrls.Add(existingUrl);
            await dbContext.SaveChangesAsync();
        });

        const string imageLid1 = "image-1";
        const string imageLid2 = "image-2";
        const string paragraphLid = "para-1";
        const string blockLid = "block-1";

        var requestBody = new
        {
            atomic__operations = new object[]
            {
                // create resource
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "webImages",
                        lid = imageLid1,
                        attributes = new
                        {
                            path = newImagePath1
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "webImages",
                        lid = imageLid2,
                        attributes = new
                        {
                            path = newImagePath2
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "paragraphs",
                        lid = paragraphLid,
                        attributes = new
                        {
                            text = newParagraphText
                        },
                        relationships = new
                        {
                            topImage = new
                            {
                                data = new
                                {
                                    type = "webImages",
                                    lid = imageLid1
                                }
                            }
                        }
                    }
                },
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "textBlocks",
                        lid = blockLid,
                        attributes = new
                        {
                            columnCount = newBlockColumnCount
                        },
                        relationships = new
                        {
                            paragraphs = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "paragraphs",
                                        lid = paragraphLid
                                    }
                                }
                            }
                        }
                    }
                },
                // update resource
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "webImages",
                        lid = imageLid1,
                        attributes = new
                        {
                            description = newImageDescription
                        }
                    }
                },
                new
                {
                    op = "update",
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
                            },
                            content = new
                            {
                                data = new[]
                                {
                                    new
                                    {
                                        type = "textBlocks",
                                        lid = blockLid
                                    }
                                }
                            }
                        }
                    }
                },
                // delete resource
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "webImages",
                        lid = imageLid1
                    }
                },
                // set relationship
                // Fix: the next operation fails, because the previous delete operation updated Paragraph, which we didn't track.
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "paragraphs",
                        lid = paragraphLid,
                        relationship = "topImage"
                    },
                    data = new
                    {
                        type = "webImages",
                        lid = imageLid2
                    }
                }
                /*
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "paragraphs",
                        lid = paragraphLid,
                        relationship = "usedIn"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "textBlocks",
                            lid = blockLid
                        }
                    }
                },
                new
                {
                    op = "update",
                    data = new
                    {
                        type = "paragraphs",
                        lid = paragraphLid,
                        attributes = new
                        {
                            text = newParagraphText
                        },
                        relationships = new
                        {
                            usedIn = new
                            {
                                data = Array.Empty<object>()
                            }
                        }
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "textBlocks",
                        lid = blockLid
                    }
                }*/
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
