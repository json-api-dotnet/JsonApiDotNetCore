using System;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class RequestScopedServiceProviderTests
    {
        [Fact]
        public void When_http_context_is_unavailable_it_must_fail()
        {
            // Arrange
            var provider = new RequestScopedServiceProvider(new HttpContextAccessor());

            // Act
            Action action = () => provider.GetService(typeof(IIdentifiable<Tag>));

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(action);

            Assert.StartsWith("Cannot resolve scoped service " +
                "'JsonApiDotNetCore.Models.IIdentifiable`1[[JsonApiDotNetCoreExample.Models.Tag, JsonApiDotNetCoreExample, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]' " +
                "outside the context of an HTTP request.", exception.Message);
        }
    }
}
