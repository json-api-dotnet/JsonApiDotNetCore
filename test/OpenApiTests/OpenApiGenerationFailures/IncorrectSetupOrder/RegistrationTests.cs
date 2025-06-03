using FluentAssertions;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.OpenApi.Swashbuckle;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OpenApiTests.OpenApiGenerationFailures.IncorrectSetupOrder;

public sealed class RegistrationTests
{
    [Fact]
    public void Fails_when_OpenAPI_registered_without_JsonApi()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action action = () => services.AddOpenApiForJsonApi();

        // Arrange
        action.Should().ThrowExactly<InvalidConfigurationException>()
            .WithMessage("Call 'services.AddJsonApi()' before calling 'services.AddOpenApiForJsonApi()'.");
    }
}
