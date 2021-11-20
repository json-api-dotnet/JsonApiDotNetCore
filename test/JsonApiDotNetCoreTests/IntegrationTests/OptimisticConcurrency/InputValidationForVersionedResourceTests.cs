using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

public sealed class InputValidationForVersionedResourceTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> _testContext;
    private readonly ConcurrencyFakers _fakers = new();

    public InputValidationForVersionedResourceTests(IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> testContext)
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
    public async Task Cannot_create_resource_with_version()
    {
        // Arrange
        string newParagraphText = _fakers.Paragraph.Generate().Text;

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                version = Unknown.Version,
                attributes = new
                {
                    text = newParagraphText
                }
            }
        };

        const string route = "/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/version");
    }

    [Fact]
    public async Task Cannot_create_resource_without_version_in_ToOne_relationship()
    {
        // Arrange
        WebImage existingImage = _fakers.WebImage.Generate();

        string newParagraphText = _fakers.Paragraph.Generate().Text;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.WebImages.Add(existingImage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
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
                            id = existingImage.StringId
                        }
                    }
                }
            }
        };

        const string route = "/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/topImage/data");
    }

    [Fact]
    public async Task Cannot_create_resource_without_version_in_ToMany_relationship()
    {
        // Arrange
        TextBlock existingBlock = _fakers.TextBlock.Generate();

        string newParagraphText = _fakers.Paragraph.Generate().Text;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.TextBlocks.Add(existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                attributes = new
                {
                    text = newParagraphText
                },
                relationships = new
                {
                    usedIn = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "textBlocks",
                                id = existingBlock.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/paragraphs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/usedIn/data[0]");
    }

    [Fact]
    public async Task Cannot_update_resource_without_version_in_url()
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
                version = existingParagraph.Version,
                attributes = new
                {
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is required at this endpoint.");
        error.Detail.Should().Be("Resources of type 'paragraphs' require the version to be specified.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_resource_without_version_in_body()
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
                attributes = new
                {
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
    }

    [Fact]
    public async Task Cannot_update_resource_with_version_mismatch_between_url_and_body()
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
                version = Unknown.Version,
                attributes = new
                {
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
        error.Title.Should().Be("Failed to deserialize request body: Conflicting 'version' values found.");
        error.Detail.Should().Be($"Expected '{existingParagraph.Version}' instead of '{Unknown.Version}'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/version");
    }

    [Fact]
    public async Task Cannot_update_resource_without_version_in_ToOne_relationship()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        WebImage existingImage = _fakers.WebImage.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingImage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = existingParagraph.StringId,
                version = existingParagraph.Version,
                relationships = new
                {
                    topImage = new
                    {
                        data = new
                        {
                            type = "webImages",
                            id = existingImage.StringId
                        }
                    }
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/topImage/data");
    }

    [Fact]
    public async Task Cannot_update_resource_without_version_in_ToMany_relationship()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        TextBlock existingBlock = _fakers.TextBlock.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "paragraphs",
                id = existingParagraph.StringId,
                version = existingParagraph.Version,
                relationships = new
                {
                    usedIn = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "textBlocks",
                                id = existingBlock.StringId
                            }
                        }
                    }
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/usedIn/data[0]");
    }

    [Fact]
    public async Task Cannot_update_relationship_without_version_in_url()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        WebImage existingImage = _fakers.WebImage.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingImage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "webImages",
                id = existingImage.StringId,
                version = existingImage.Version
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId}/relationships/topImage";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is required at this endpoint.");
        error.Detail.Should().Be("Resources of type 'paragraphs' require the version to be specified.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_ToOne_relationship_without_version_in_body()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        WebImage existingImage = _fakers.WebImage.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingImage);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "webImages",
                id = existingImage.StringId
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}/relationships/topImage";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
    }

    [Fact]
    public async Task Cannot_update_ToMany_relationship_without_version_in_body()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        TextBlock existingBlock = _fakers.TextBlock.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "textBlocks",
                    id = existingBlock.StringId
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_without_version_in_url()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        TextBlock existingBlock = _fakers.TextBlock.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "textBlocks",
                    id = existingBlock.StringId,
                    version = existingBlock.Version
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is required at this endpoint.");
        error.Detail.Should().Be("Resources of type 'paragraphs' require the version to be specified.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_without_version_in_body()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        TextBlock existingBlock = _fakers.TextBlock.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "textBlocks",
                    id = existingBlock.StringId
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_without_version_in_url()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        TextBlock existingBlock = _fakers.TextBlock.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "textBlocks",
                    id = existingBlock.StringId,
                    version = existingBlock.Version
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is required at this endpoint.");
        error.Detail.Should().Be("Resources of type 'paragraphs' require the version to be specified.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_without_version_in_body()
    {
        // Arrange
        Paragraph existingParagraph = _fakers.Paragraph.Generate();

        TextBlock existingBlock = _fakers.TextBlock.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingParagraph, existingBlock);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "textBlocks",
                    id = existingBlock.StringId
                }
            }
        };

        string route = $"/paragraphs/{existingParagraph.StringId};v~{existingParagraph.Version}/relationships/usedIn";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'version' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]");
    }
}
