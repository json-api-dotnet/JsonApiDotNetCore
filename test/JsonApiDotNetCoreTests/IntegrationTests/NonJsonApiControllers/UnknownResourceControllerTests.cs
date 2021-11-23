using System;
using FluentAssertions;
using JsonApiDotNetCore.Errors;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers
{
    public sealed class UnknownResourceControllerTests : IntegrationTestContext<TestableStartup<NonJsonApiDbContext>, NonJsonApiDbContext>
    {
        public UnknownResourceControllerTests()
        {
            UseController<UnknownResourcesController>();
        }

        [Fact]
        public void Fails_at_startup_when_using_controller_for_resource_type_that_is_not_registered_in_resource_graph()
        {
            // Act
            Action action = () => _ = Factory;

            // Assert
            action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage($"Controller '{typeof(UnknownResourcesController)}' " +
                $"depends on resource type '{typeof(UnknownResource)}', which does not exist in the resource graph.");
        }

        public override void Dispose()
        {
            // Prevents crash when test cleanup tries to access lazily constructed Factory.
        }
    }
}
