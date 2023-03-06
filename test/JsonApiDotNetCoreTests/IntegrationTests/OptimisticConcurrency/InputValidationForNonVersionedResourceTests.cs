using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

public sealed class InputValidationForNonVersionedResourceTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> _testContext;
    private readonly ConcurrencyFakers _fakers = new();

    public InputValidationForNonVersionedResourceTests(IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DeploymentJobsController>();
    }

    [Fact]
    public async Task Cannot_create_resource_with_version_in_ToOne_relationship()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        DateTimeOffset? newJobStartedAt = _fakers.DeploymentJob.Generate().StartedAt;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                attributes = new
                {
                    startedAt = newJobStartedAt
                },
                relationships = new
                {
                    parentJob = new
                    {
                        data = new
                        {
                            type = "deploymentJobs",
                            id = existingJob.StringId,
                            version = Unknown.Version
                        }
                    }
                }
            }
        };

        const string route = "/deploymentJobs";

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
        error.Source.Pointer.Should().Be("/data/relationships/parentJob/data/version");
    }

    [Fact]
    public async Task Cannot_create_resource_with_version_in_ToMany_relationship()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        DateTimeOffset? newJobStartedAt = _fakers.DeploymentJob.Generate().StartedAt;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                attributes = new
                {
                    startedAt = newJobStartedAt
                },
                relationships = new
                {
                    childJobs = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "deploymentJobs",
                                id = existingJob.StringId,
                                version = Unknown.Version
                            }
                        }
                    }
                }
            }
        };

        const string route = "/deploymentJobs";

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
        error.Source.Pointer.Should().Be("/data/relationships/childJobs/data[0]/version");
    }

    [Fact]
    public async Task Cannot_update_resource_with_version_in_url()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                id = existingJob.StringId,
                attributes = new
                {
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId};v~{Unknown.Version}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is not supported at this endpoint.");
        error.Detail.Should().Be("Resources of type 'deploymentJobs' are not versioned.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_resource_with_version_in_body()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                id = existingJob.StringId,
                version = Unknown.Version,
                attributes = new
                {
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

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
    public async Task Cannot_update_resource_with_version_in_ToOne_relationship()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                id = existingJob.StringId,
                relationships = new
                {
                    parentJob = new
                    {
                        data = new
                        {
                            type = "deploymentJobs",
                            id = existingJob.StringId,
                            version = Unknown.Version
                        }
                    }
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/parentJob/data/version");
    }

    [Fact]
    public async Task Cannot_update_resource_with_version_in_ToMany_relationship()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                id = existingJob.StringId,
                relationships = new
                {
                    childJobs = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "deploymentJobs",
                                id = existingJob.StringId,
                                version = Unknown.Version
                            }
                        }
                    }
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/childJobs/data[0]/version");
    }

    [Fact]
    public async Task Cannot_update_relationship_with_version_in_url()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                id = existingJob.StringId
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId};v~{Unknown.Version}/relationships/parentJob";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is not supported at this endpoint.");
        error.Detail.Should().Be("Resources of type 'deploymentJobs' are not versioned.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_update_ToOne_relationship_with_version_in_body()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "deploymentJobs",
                id = existingJob.StringId,
                version = Unknown.Version
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}/relationships/parentJob";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

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
    public async Task Cannot_update_ToMany_relationship_with_version_in_body()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "deploymentJobs",
                    id = existingJob.StringId,
                    version = Unknown.Version
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}/relationships/childJobs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]/version");
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_with_version_in_url()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "deploymentJobs",
                    id = existingJob.StringId
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId};v~{Unknown.Version}/relationships/childJobs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is not supported at this endpoint.");
        error.Detail.Should().Be("Resources of type 'deploymentJobs' are not versioned.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_add_to_ToMany_relationship_with_version_in_body()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "deploymentJobs",
                    id = existingJob.StringId,
                    version = Unknown.Version
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}/relationships/childJobs";

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
        error.Source.Pointer.Should().Be("/data[0]/version");
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_with_version_in_url()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();
        existingJob.ChildJobs = _fakers.DeploymentJob.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "deploymentJobs",
                    id = existingJob.ChildJobs[0].StringId
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId};v~{Unknown.Version}/relationships/childJobs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("The 'version' parameter is not supported at this endpoint.");
        error.Detail.Should().Be("Resources of type 'deploymentJobs' are not versioned.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_remove_from_ToMany_relationship_with_version_in_body()
    {
        // Arrange
        DeploymentJob existingJob = _fakers.DeploymentJob.Generate();
        existingJob.ChildJobs = _fakers.DeploymentJob.Generate(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DeploymentJobs.Add(existingJob);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "deploymentJobs",
                    id = existingJob.ChildJobs[0].StringId,
                    version = Unknown.Version
                }
            }
        };

        string route = $"/deploymentJobs/{existingJob.StringId}/relationships/childJobs";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data[0]/version");
    }
}
