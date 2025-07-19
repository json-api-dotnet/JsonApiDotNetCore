#if !NET8_0
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DiscoveryTests;

public sealed class AspNetOpenApiTests
{
    [Fact]
    public async Task Throws_when_AspNet_OpenApi_is_registered()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.WebHost.UseTestServer();
        builder.Services.AddJsonApi();
        builder.Services.AddOpenApi();
        await using WebApplication app = builder.Build();

        // Act
        Action action = app.UseJsonApi;

        // Assert
        action.Should().ThrowExactly<InvalidConfigurationException>().WithMessage("JsonApiDotNetCore is incompatible with ASP.NET OpenAPI. " +
            "Remove 'services.AddOpenApi()', or replace it by calling 'services.AddOpenApiForJsonApi()' after 'services.AddJsonApi()' " +
            "from the JsonApiDotNetCore.OpenApi.Swashbuckle NuGet package.");
    }
}
#endif
