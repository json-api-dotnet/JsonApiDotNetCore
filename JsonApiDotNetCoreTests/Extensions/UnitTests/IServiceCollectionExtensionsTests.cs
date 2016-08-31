using Xunit;
using JsonApiDotNetCore.Routing;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCoreTests.Helpers;
using Microsoft.EntityFrameworkCore;
using System;

namespace JsonApiDotNetCoreTests.Extensions.UnitTests
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddJsonApi_AddsRouterToServiceCollection()
        {
            // arrange
            var serviceCollection = new ServiceCollection();

            // act
            serviceCollection.AddJsonApi(config =>
            {
                config.UseContext<DbContext>();
            });

            // assert
            Assert.True(serviceCollection.ContainsType(typeof(IRouter)));
        }

        [Fact]
        public void AddJsonApi_ThrowsException_IfContextIsNotDefined()
        {
            // arrange
            var serviceCollection = new ServiceCollection();

            // act
            var testAction = new Action(() =>
            {
                serviceCollection.AddJsonApi(config => { });
            });

            // assert
            Assert.Throws<NullReferenceException>(testAction);
        }
    }
}
