using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Controllers;

public sealed class AtomicDefaultConstrainedOperationsControllerTests
    : IClassFixture<IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers = new();

    public AtomicDefaultConstrainedOperationsControllerTests(IntegrationTestContext<TestableStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Cannot_delete_resource_for_inaccessible_operation()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingLanguage);
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
                        type = "textLanguages",
                        id = existingLanguage.StringId
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("The requested operation is not accessible.");
        error.Detail.Should().Be("The 'remove' resource operation is not accessible for resource type 'textLanguages'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations[0]");
    }

    [Fact]
    public async Task Cannot_change_ToMany_relationship_for_inaccessible_operations()
    {
        // Arrange
        TextLanguage existingLanguage = _fakers.TextLanguage.GenerateOne();
        Lyric existingLyric = _fakers.Lyric.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.AddInRange(existingLanguage, existingLyric);
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
                        type = "textLanguages",
                        id = existingLanguage.StringId,
                        relationship = "lyrics"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
                        }
                    }
                },
                new
                {
                    op = "add",
                    @ref = new
                    {
                        type = "textLanguages",
                        id = existingLanguage.StringId,
                        relationship = "lyrics"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
                        }
                    }
                },
                new
                {
                    op = "remove",
                    @ref = new
                    {
                        type = "textLanguages",
                        id = existingLanguage.StringId,
                        relationship = "lyrics"
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "lyrics",
                            id = existingLyric.StringId
                        }
                    }
                }
            }
        };

        const string route = "/operations";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAtomicAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(3);

        ErrorObject error1 = responseDocument.Errors[0];
        error1.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error1.Title.Should().Be("The requested operation is not accessible.");
        error1.Detail.Should().Be("The 'update' relationship operation is not accessible for relationship 'lyrics' on resource type 'textLanguages'.");
        error1.Source.ShouldNotBeNull();
        error1.Source.Pointer.Should().Be("/atomic:operations[0]");

        ErrorObject error2 = responseDocument.Errors[1];
        error2.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error2.Title.Should().Be("The requested operation is not accessible.");
        error2.Detail.Should().Be("The 'add' relationship operation is not accessible for relationship 'lyrics' on resource type 'textLanguages'.");
        error2.Source.ShouldNotBeNull();
        error2.Source.Pointer.Should().Be("/atomic:operations[1]");

        ErrorObject error3 = responseDocument.Errors[2];
        error3.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error3.Title.Should().Be("The requested operation is not accessible.");
        error3.Detail.Should().Be("The 'remove' relationship operation is not accessible for relationship 'lyrics' on resource type 'textLanguages'.");
        error3.Source.ShouldNotBeNull();
        error3.Source.Pointer.Should().Be("/atomic:operations[2]");
    }
}
