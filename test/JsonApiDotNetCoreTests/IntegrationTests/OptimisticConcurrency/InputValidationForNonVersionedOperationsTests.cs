using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

public sealed class InputValidationForNonVersionedOperationsTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> _testContext;
    private readonly ConcurrencyFakers _fakers = new();

    public InputValidationForNonVersionedOperationsTests(IntegrationTestContext<TestableStartup<ConcurrencyDbContext>, ConcurrencyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
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
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
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
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/parentJob/data/version");
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
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
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
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/relationships/childJobs/data[0]/version");
    }

    [Fact]
    public async Task Cannot_update_resource_with_version_in_ref()
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
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
                        version = Unknown.Version
                    },
                    data = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/version");
    }

    [Fact]
    public async Task Cannot_update_resource_with_version_in_data()
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
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId
                    },
                    data = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
                        version = Unknown.Version,
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/version");
    }

    [Fact]
    public async Task Cannot_delete_resource_with_version()
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
            atomic__operations = new[]
            {
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
                        version = Unknown.Version
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/version");
    }

    [Fact]
    public async Task Cannot_update_relationship_with_version_in_ref()
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
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
                        version = Unknown.Version,
                        relationship = "parentJob"
                    },
                    data = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/ref/version");
    }

    [Fact]
    public async Task Cannot_update_relationship_with_version_in_data()
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
            atomic__operations = new[]
            {
                new
                {
                    op = "update",
                    @ref = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
                        relationship = "parentJob"
                    },
                    data = new
                    {
                        type = "deploymentJobs",
                        id = existingJob.StringId,
                        version = Unknown.Version
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unexpected 'version' element.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]/data/version");
    }
}
