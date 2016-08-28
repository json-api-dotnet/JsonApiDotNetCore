using Xunit;
using JsonApiDotNetCore.Routing;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCoreTests.Helpers;

namespace JsonApiDotNetCoreTests.Extensions.UnitTests
{
    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dotnet-core.html
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddJsonApi_AddsRouterToServiceCollection()
        {
            // arrange
            var serviceCollection = new ServiceCollection();

            // act
            serviceCollection.AddJsonApi(config => {});

            // assert
            Assert.True(serviceCollection.ContainsType(typeof(Router)));
        }
    }
}
