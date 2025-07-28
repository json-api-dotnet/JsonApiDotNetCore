using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using OpenApiTests;
using OpenApiTests.AtomicOperations;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiNSwagEndToEndTests.AtomicOperations;

public sealed class MediaTypeTests : IClassFixture<IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> _testContext;
    private readonly OperationsFakers _fakers;

    public MediaTypeTests(IntegrationTestContext<OpenApiStartup<OperationsDbContext>, OperationsDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();

        _fakers = new OperationsFakers(testContext.Factory.Services);
    }

    [Fact]
    public async Task Can_create_resource_with_default_media_type()
    {
        // Arrange
        Teacher newTeacher = _fakers.Teacher.GenerateOne();

        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "teachers",
                        attributes = new
                        {
                            name = newTeacher.Name,
                            emailAddress = newTeacher.EmailAddress
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

        httpResponse.Content.Headers.ContentType.Should().NotBeNull();
        httpResponse.Content.Headers.ContentType.ToString().Should().Be(JsonApiMediaType.AtomicOperations.ToString());

        responseDocument.Results.Should().HaveCount(1);

        responseDocument.Results[0].Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Type.Should().Be("teachers");
            resource.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(newTeacher.Name);
            resource.Attributes.Should().ContainKey("emailAddress").WhoseValue.Should().Be(newTeacher.EmailAddress);
            resource.Relationships.Should().BeNull();
        });

        long newTeacherId = long.Parse(responseDocument.Results[0].Data.SingleValue!.Id.Should().NotBeNull().And.Subject);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Teacher teacherInDatabase = await dbContext.Teachers.FirstWithIdAsync(newTeacherId);

            teacherInDatabase.Name.Should().Be(newTeacher.Name);
            teacherInDatabase.EmailAddress.Should().Be(newTeacher.EmailAddress);
        });
    }
}
