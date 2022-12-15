using FluentAssertions;
using JsonApiDotNetCore.Errors;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

public sealed class DuplicateResourceControllerTests : IntegrationTestContext<TestableStartup<KnownDbContext>, KnownDbContext>
{
    public DuplicateResourceControllerTests()
    {
        UseController<KnownResourcesController>();
        UseController<DuplicateKnownResourcesController>();
    }

    [Fact]
    public void Fails_at_startup_when_multiple_controllers_exist_for_same_resource_type()
    {
        // Act
        Action action = () => _ = Factory;

        // Assert
        InvalidConfigurationException exception = action.Should().ThrowExactly<InvalidConfigurationException>().Which!;
        exception.Message.Should().StartWith("Multiple controllers found for resource type 'knownResources': ");
        exception.Message.Should().Contain($"'{typeof(KnownResourcesController).FullName}'");
        exception.Message.Should().Contain($"'{typeof(DuplicateKnownResourcesController).FullName}'");
    }
}
