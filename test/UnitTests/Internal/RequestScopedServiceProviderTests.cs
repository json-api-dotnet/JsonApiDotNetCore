using System;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
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
            Action action = () => provider.GetService(typeof(AppDbContext));

            // Assert
            var exception = Assert.Throws<ResolveScopedServiceRequiresHttpContextException>(action);

            Assert.Equal(HttpStatusCode.InternalServerError, exception.Error.StatusCode);
            Assert.Equal("Cannot resolve scoped service outside the context of an HTTP request.", exception.Error.Title);
            Assert.StartsWith("Type requested was 'JsonApiDotNetCoreExample.Data.AppDbContext'. If you are hitting this error in automated tests", exception.Error.Detail);
            Assert.Equal(typeof(AppDbContext), exception.ServiceType);
        }
    }
}
