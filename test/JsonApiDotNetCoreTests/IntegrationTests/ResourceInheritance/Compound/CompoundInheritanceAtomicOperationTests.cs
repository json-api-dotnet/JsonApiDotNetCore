using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.TablePerHierarchy;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound;

public sealed class CompoundInheritanceAtomicOperationTests
    : IClassFixture<IntegrationTestContext<TestableStartup<CompoundTablePerHierarchyDbContext>, CompoundTablePerHierarchyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<CompoundTablePerHierarchyDbContext>, CompoundTablePerHierarchyDbContext> _testContext;

    public CompoundInheritanceAtomicOperationTests(
        IntegrationTestContext<TestableStartup<CompoundTablePerHierarchyDbContext>, CompoundTablePerHierarchyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<OperationsController>();
    }

    [Fact]
    public async Task Atomic_create_apartment_with_invalid_nested_member()
    {
        // Arrange
        var requestBody = new
        {
            atomic__operations = new[]
            {
                new
                {
                    op = "add",
                    data = new
                    {
                        type = "apartments",
                        attributes = new
                        {
                            title = "Some apartment",
                            building = new
                            {
                                name = "Tower",
                                floor = 0
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

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field Floor must be between 1 and 100.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/atomic:operations/0/data/attributes/building/floor");
    }
}
