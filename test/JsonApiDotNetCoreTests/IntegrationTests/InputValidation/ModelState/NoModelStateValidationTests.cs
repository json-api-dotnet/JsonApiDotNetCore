using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.Startups;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

public sealed class NoModelStateValidationTests : IClassFixture<IntegrationTestContext<NoModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext>>
{
    private readonly IntegrationTestContext<NoModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext> _testContext;
    private readonly ModelStateFakers _fakers = new();

    public NoModelStateValidationTests(IntegrationTestContext<NoModelStateValidationStartup<ModelStateDbContext>, ModelStateDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<SystemVolumesController>();
        testContext.UseController<SystemDirectoriesController>();
        testContext.UseController<SystemFilesController>();
    }

    [Fact]
    public async Task Can_create_resource_with_invalid_attribute_value()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "systemDirectories",
                attributes = new
                {
                    directoryName = "!@#$%^&*().-",
                    isCaseSensitive = false
                }
            }
        };

        const string route = "/systemDirectories";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("directoryName").WhoseValue.Should().Be("!@#$%^&*().-");
    }

    [Fact]
    public async Task Can_update_resource_with_invalid_attribute_value()
    {
        // Arrange
        SystemDirectory existingDirectory = _fakers.SystemDirectory.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Directories.Add(existingDirectory);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "systemDirectories",
                id = existingDirectory.StringId,
                attributes = new
                {
                    directoryName = "!@#$%^&*().-"
                }
            }
        };

        string route = $"/systemDirectories/{existingDirectory.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_clear_required_OneToOne_relationship_at_primary_endpoint()
    {
        // Arrange
        SystemVolume existingVolume = _fakers.SystemVolume.GenerateOne();
        existingVolume.RootDirectory = _fakers.SystemDirectory.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Volumes.Add(existingVolume);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "systemVolumes",
                id = existingVolume.StringId,
                relationships = new
                {
                    rootDirectory = new
                    {
                        data = (object?)null
                    }
                }
            }
        };

        string route = $"/systemVolumes/{existingVolume.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to clear a required relationship.");
        error.Detail.Should().Be("The relationship 'rootDirectory' on resource type 'systemVolumes' cannot be cleared because it is a required relationship.");
    }
}
