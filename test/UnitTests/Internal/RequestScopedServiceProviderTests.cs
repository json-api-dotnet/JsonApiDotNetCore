using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class RequestScopedServiceProviderTests
    {
        [Fact]
        public void When_http_context_is_unavailable_it_must_fail()
        {
            // Arrange
            Type serviceType = typeof(IIdentifiable<Model>);

            var provider = new RequestScopedServiceProvider(new HttpContextAccessor());

            // Act
            Action action = () => provider.GetRequiredService(serviceType);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(action);

            Assert.StartsWith($"Cannot resolve scoped service '{serviceType.FullName}' outside the context of an HTTP request.", exception.Message);
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Itself)]
        private sealed class Model : Identifiable
        {
        }
    }
}
