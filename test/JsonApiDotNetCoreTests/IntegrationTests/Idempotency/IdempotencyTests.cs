using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Idempotency;

public sealed class IdempotencyTests : IClassFixture<IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> _testContext;
    private readonly IdempotencyFakers _fakers = new();

    public IdempotencyTests(IntegrationTestContext<TestableStartup<IdempotencyDbContext>, IdempotencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<TreesController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddScoped<IIdempotencyProvider, IdempotencyProvider>();
            services.AddScoped<ISystemClock, FrozenSystemClock>();
        });
    }

    [Fact]
    public async Task Returns_cached_response_for_create_resource_request()
    {
        // Arrange
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        var requestBody = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters
                }
            }
        };

        const string route = "/trees";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, Document responseDocument1) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            long newTreeId = long.Parse(responseDocument1.Data.SingleValue!.Id.ShouldNotBeNull());
            Tree existingTree = await dbContext.Trees.FirstWithIdAsync(newTreeId);

            existingTree.HeightInMeters *= 2;
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.Created);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.Created);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().Be(idempotencyKey.DoubleQuote());

        httpResponse2.Headers.Location.Should().Be(httpResponse1.Headers.Location);

        httpResponse2.Content.Headers.ContentType.Should().Be(httpResponse1.Content.Headers.ContentType);
        httpResponse2.Content.Headers.ContentLength.Should().Be(httpResponse1.Content.Headers.ContentLength);

        responseDocument1.Data.SingleValue.ShouldNotBeNull();
        object? height1 = responseDocument1.Data.SingleValue.Attributes.ShouldContainKey("heightInMeters");

        responseDocument2.Data.SingleValue.ShouldNotBeNull();
        responseDocument2.Data.SingleValue.Id.Should().Be(responseDocument1.Data.SingleValue.Id);
        responseDocument2.Data.SingleValue.Attributes.ShouldContainKey("heightInMeters").With(value => value.Should().Be(height1));
    }

    [Fact]
    public async Task Returns_cached_response_for_failed_create_resource_request()
    {
        // Arrange
        var requestBody = new
        {
            data = new object()
        };

        const string route = "/trees";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecutePostAsync<string>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecutePostAsync<string>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().Be(idempotencyKey.DoubleQuote());

        httpResponse2.Content.Headers.ContentType.Should().Be(httpResponse1.Content.Headers.ContentType);
        httpResponse2.Content.Headers.ContentLength.Should().Be(httpResponse1.Content.Headers.ContentLength);

        responseDocument2.Should().Be(responseDocument1);
    }

    [Fact]
    public async Task Cannot_create_resource_without_idempotency_key()
    {
        // Arrange
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        var requestBody = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters
                }
            }
        };

        const string route = "/trees";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be($"Missing '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().StartWith("An idempotency key is a unique value generated by the client,");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_create_resource_with_empty_idempotency_key()
    {
        // Arrange
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        var requestBody = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters
                }
            }
        };

        const string route = "/trees";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, string.Empty.DoubleQuote());
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be($"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().Be("Expected non-empty value surrounded by double quotes.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(HeaderConstants.IdempotencyKey);
    }

    [Fact]
    public async Task Cannot_create_resource_with_unquoted_idempotency_key()
    {
        // Arrange
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        var requestBody = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters
                }
            }
        };

        const string route = "/trees";

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, Guid.NewGuid().ToString());
        };

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be($"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().Be("Expected non-empty value surrounded by double quotes.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(HeaderConstants.IdempotencyKey);
    }

    [Fact]
    public async Task Cannot_reuse_idempotency_key_for_different_request_url()
    {
        // Arrange
        decimal newHeightInMeters = _fakers.Tree.Generate().HeightInMeters;

        var requestBody = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters
                }
            }
        };

        const string route1 = "/trees";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, _) = await _testContext.ExecutePostAsync<Document>(route1, requestBody, setRequestHeaders: setRequestHeaders);

        const string route2 = "/branches";

        // Act
        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecutePostAsync<Document>(route2, requestBody, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.Created);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.Location.Should().BeNull();
        httpResponse2.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse2.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.MediaType);

        responseDocument2.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument2.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be($"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().Be($"The provided idempotency key '{idempotencyKey}' is in use for another request.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(HeaderConstants.IdempotencyKey);
    }

    [Fact]
    public async Task Cannot_reuse_idempotency_key_for_different_request_body()
    {
        // Arrange
        decimal newHeightInMeters1 = _fakers.Tree.Generate().HeightInMeters;
        decimal newHeightInMeters2 = _fakers.Tree.Generate().HeightInMeters;

        var requestBody1 = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters1
                }
            }
        };

        const string route = "/trees";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, _) = await _testContext.ExecutePostAsync<Document>(route, requestBody1, setRequestHeaders: setRequestHeaders);

        var requestBody2 = new
        {
            data = new
            {
                type = "trees",
                attributes = new
                {
                    heightInMeters = newHeightInMeters2
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecutePostAsync<Document>(route, requestBody2, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.Created);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.Location.Should().BeNull();
        httpResponse2.Content.Headers.ContentType.ShouldNotBeNull();
        httpResponse2.Content.Headers.ContentType.ToString().Should().Be(HeaderConstants.MediaType);

        responseDocument2.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument2.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be($"Invalid '{HeaderConstants.IdempotencyKey}' HTTP header.");
        error.Detail.Should().Be($"The provided idempotency key '{idempotencyKey}' is in use for another request.");
        error.Source.ShouldNotBeNull();
        error.Source.Header.Should().Be(HeaderConstants.IdempotencyKey);
    }

    [Fact]
    public async Task Ignores_idempotency_key_on_GET_request()
    {
        // Arrange
        Tree tree = _fakers.Tree.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Trees.Add(tree);
            await dbContext.SaveChangesAsync();
        });

        string route = "/trees/" + tree.StringId;

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, Document responseDocument1) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tree existingTree = await dbContext.Trees.FirstWithIdAsync(tree.Id);

            existingTree.HeightInMeters *= 2;
            await dbContext.SaveChangesAsync();
        });

        // Act
        (HttpResponseMessage httpResponse2, Document responseDocument2) = await _testContext.ExecuteGetAsync<Document>(route, setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.OK);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.OK);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();

        responseDocument1.Data.SingleValue.ShouldNotBeNull();
        object? height1 = responseDocument1.Data.SingleValue.Attributes.ShouldContainKey("heightInMeters");

        responseDocument2.Data.SingleValue.ShouldNotBeNull();
        responseDocument2.Data.SingleValue.Id.Should().Be(responseDocument1.Data.SingleValue.Id);
        responseDocument2.Data.SingleValue.Attributes.ShouldContainKey("heightInMeters").With(value => value.Should().NotBe(height1));
    }

    [Fact]
    public async Task Ignores_idempotency_key_on_PATCH_resource_request()
    {
        // Arrange
        Tree existingTree = _fakers.Tree.Generate();

        decimal newHeightInMeters1 = _fakers.Tree.Generate().HeightInMeters;
        decimal newHeightInMeters2 = _fakers.Tree.Generate().HeightInMeters;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Trees.Add(existingTree);
            await dbContext.SaveChangesAsync();
        });

        var requestBody1 = new
        {
            data = new
            {
                type = "trees",
                id = existingTree.StringId,
                attributes = new
                {
                    heightInMeters = newHeightInMeters1
                }
            }
        };

        string route = "/trees/" + existingTree.StringId;

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecutePatchAsync<string>(route, requestBody1, setRequestHeaders: setRequestHeaders);

        var requestBody2 = new
        {
            data = new
            {
                type = "trees",
                id = existingTree.StringId,
                attributes = new
                {
                    heightInMeters = newHeightInMeters2
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecutePatchAsync<string>(route, requestBody2, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();

        responseDocument1.Should().BeEmpty();
        responseDocument2.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tree treeInDatabase = await dbContext.Trees.FirstWithIdAsync(existingTree.Id);

            treeInDatabase.HeightInMeters.Should().BeApproximately(requestBody2.data.attributes.heightInMeters);
        });
    }

    [Fact]
    public async Task Ignores_idempotency_key_on_DELETE_resource_request()
    {
        // Arrange
        Tree existingTree = _fakers.Tree.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Trees.Add(existingTree);
            await dbContext.SaveChangesAsync();
        });

        string route = "/trees/" + existingTree.StringId;

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecuteDeleteAsync<string>(route, setRequestHeaders: setRequestHeaders);

        // Act
        (HttpResponseMessage httpResponse2, Document responseDocument2) =
            await _testContext.ExecuteDeleteAsync<Document>(route, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();

        responseDocument1.Should().BeEmpty();
        responseDocument2.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task Ignores_idempotency_key_on_PATCH_relationship_request()
    {
        // Arrange
        Tree existingTree = _fakers.Tree.Generate();
        existingTree.Branches = _fakers.Branch.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Trees.Add(existingTree);
            await dbContext.SaveChangesAsync();
        });

        var requestBody1 = new
        {
            data = new[]
            {
                new
                {
                    type = "branches",
                    id = existingTree.Branches[0].StringId
                }
            }
        };

        string route = $"/trees/{existingTree.StringId}/relationships/branches";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecutePatchAsync<string>(route, requestBody1, setRequestHeaders: setRequestHeaders);

        var requestBody2 = new
        {
            data = new[]
            {
                new
                {
                    type = "branches",
                    id = existingTree.Branches[1].StringId
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecutePatchAsync<string>(route, requestBody2, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();

        responseDocument1.Should().BeEmpty();
        responseDocument2.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tree treeInDatabase = await dbContext.Trees.Include(tree => tree.Branches).FirstWithIdAsync(existingTree.Id);

            treeInDatabase.Branches.Should().HaveCount(1);
            treeInDatabase.Branches[0].Id.Should().Be(existingTree.Branches[1].Id);
        });
    }

    [Fact]
    public async Task Ignores_idempotency_key_on_POST_relationship_request()
    {
        // Arrange
        Tree existingTree = _fakers.Tree.Generate();
        List<Branch> existingBranches = _fakers.Branch.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Trees.Add(existingTree);
            dbContext.Branches.AddRange(existingBranches);
            await dbContext.SaveChangesAsync();
        });

        var requestBody1 = new
        {
            data = new[]
            {
                new
                {
                    type = "branches",
                    id = existingBranches[0].StringId
                }
            }
        };

        string route = $"/trees/{existingTree.StringId}/relationships/branches";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecutePostAsync<string>(route, requestBody1, setRequestHeaders: setRequestHeaders);

        var requestBody2 = new
        {
            data = new[]
            {
                new
                {
                    type = "branches",
                    id = existingBranches[1].StringId
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecutePostAsync<string>(route, requestBody2, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();

        responseDocument1.Should().BeEmpty();
        responseDocument2.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tree treeInDatabase = await dbContext.Trees.Include(tree => tree.Branches).FirstWithIdAsync(existingTree.Id);

            treeInDatabase.Branches.Should().HaveCount(2);
        });
    }

    [Fact]
    public async Task Ignores_idempotency_key_on_DELETE_relationship_request()
    {
        // Arrange
        Tree existingTree = _fakers.Tree.Generate();
        existingTree.Branches = _fakers.Branch.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Trees.Add(existingTree);
            await dbContext.SaveChangesAsync();
        });

        var requestBody1 = new
        {
            data = new[]
            {
                new
                {
                    type = "branches",
                    id = existingTree.Branches[0].StringId
                }
            }
        };

        string route = $"/trees/{existingTree.StringId}/relationships/branches";

        string idempotencyKey = Guid.NewGuid().ToString();

        Action<HttpRequestHeaders> setRequestHeaders = headers =>
        {
            headers.Add(HeaderConstants.IdempotencyKey, idempotencyKey.DoubleQuote());
        };

        (HttpResponseMessage httpResponse1, string responseDocument1) =
            await _testContext.ExecuteDeleteAsync<string>(route, requestBody1, setRequestHeaders: setRequestHeaders);

        var requestBody2 = new
        {
            data = new[]
            {
                new
                {
                    type = "branches",
                    id = existingTree.Branches[1].StringId
                }
            }
        };

        // Act
        (HttpResponseMessage httpResponse2, string responseDocument2) =
            await _testContext.ExecuteDeleteAsync<string>(route, requestBody2, setRequestHeaders: setRequestHeaders);

        // Assert
        httpResponse1.ShouldHaveStatusCode(HttpStatusCode.NoContent);
        httpResponse2.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        httpResponse1.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();
        httpResponse2.Headers.GetValue(HeaderConstants.IdempotencyKey).Should().BeNull();

        responseDocument1.Should().BeEmpty();
        responseDocument2.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Tree treeInDatabase = await dbContext.Trees.Include(tree => tree.Branches).FirstWithIdAsync(existingTree.Id);

            treeInDatabase.Branches.Should().BeEmpty();
        });
    }
}
