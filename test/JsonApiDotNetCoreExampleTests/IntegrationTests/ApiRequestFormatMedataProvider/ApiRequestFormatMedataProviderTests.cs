using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Common;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    public sealed class ApiRequestFormatMedataProviderTests : IClassFixture<ExampleIntegrationTestContext<ApiExplorerStartup<ShopDbContext>, ShopDbContext>>
    {
        private readonly ExampleIntegrationTestContext<ApiExplorerStartup<ShopDbContext>, ShopDbContext> _testContext;

        public ApiRequestFormatMedataProviderTests(ExampleIntegrationTestContext<ApiExplorerStartup<ShopDbContext>, ShopDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public void Can_retrieve_request_content_type_in_ApiExplorer_when_using_ConsumesAttribute()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            List<ApiDescription> descriptions = groups.Single().Items.ToList();
            MethodInfo postStore = typeof(StoresController).GetMethod(nameof(StoresController.PostAsync));

            ApiDescription postStoreDescription = descriptions.First(description => (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo ==
                postStore);

            postStoreDescription.Should().NotBeNull();
            postStoreDescription.SupportedRequestFormats.Should().HaveCount(1);
            postStoreDescription.SupportedRequestFormats[0].MediaType.Should().Be(HeaderConstants.MediaType);
        }

        [Fact]
        public void Can_retrieve_atomic_operations_request_content_type_in_ApiExplorer_when_using_ConsumesAttribute()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            List<ApiDescription> descriptions = groups.Single().Items.ToList();
            MethodInfo postOperations = typeof(OperationsController).GetMethod(nameof(OperationsController.PostOperationsAsync));

            ApiDescription postOperationsDescription =
                descriptions.First(description => (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo == postOperations);

            postOperationsDescription.Should().NotBeNull();
            postOperationsDescription.SupportedRequestFormats.Should().HaveCount(1);
            postOperationsDescription.SupportedRequestFormats[0].MediaType.Should().Be(HeaderConstants.AtomicOperationsMediaType);
        }

        [Fact]
        public void Cannot_retrieve_request_content_type_in_ApiExplorer_without_usage_of_ConsumesAttribute()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            IReadOnlyList<ApiDescription> descriptions = groups.Single().Items;

            IEnumerable<ApiDescription> productActionDescriptions = descriptions.Where(description =>
                (description.ActionDescriptor as ControllerActionDescriptor)?.ControllerTypeInfo == typeof(ProductsController));

            foreach (ApiDescription description in productActionDescriptions)
            {
                description.SupportedRequestFormats.Should().NotContain(format => format.MediaType == HeaderConstants.MediaType);
            }
        }

        [Fact]
        public void Cannot_retrieve_atomic_operations_request_content_type_in_ApiExplorer_when_set_on_relationship_endpoint()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            List<ApiDescription> descriptions = groups.Single().Items.ToList();
            MethodInfo postRelationship = typeof(StoresController).GetMethod(nameof(StoresController.PostRelationshipAsync));

            ApiDescription postRelationshipDescription = descriptions.First(description =>
                (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo == postRelationship);

            postRelationshipDescription.Should().NotBeNull();
            postRelationshipDescription.SupportedRequestFormats.Should().HaveCount(0);
        }

        [Fact]
        public void Can_retrieve_response_content_type_in_ApiExplorer_when_using_ProducesAttribute_with_ProducesResponseTypeAttribute()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            List<ApiDescription> descriptions = groups.Single().Items.ToList();

            MethodInfo getStores = typeof(StoresController).GetMethods()
                .First(method => method.Name == nameof(StoresController.GetAsync) && method.GetParameters().Length == 1);

            ApiDescription getStoresDescription = descriptions.First(description => (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo ==
                getStores);

            getStoresDescription.Should().NotBeNull();
            getStoresDescription.SupportedResponseTypes.Should().HaveCount(1);

            ApiResponseFormat jsonApiResponse = getStoresDescription.SupportedResponseTypes[0].ApiResponseFormats
                .FirstOrDefault(format => format.Formatter.GetType().Implements(typeof(IJsonApiOutputFormatter)));

            jsonApiResponse.Should().NotBeNull();
            jsonApiResponse!.MediaType.Should().Be(HeaderConstants.MediaType);
        }

        [Fact]
        public void Cannot_retrieve_response_content_type_in_ApiExplorer_when_using_ProducesResponseTypeAttribute_without_ProducesAttribute()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            List<ApiDescription> descriptions = groups.Single().Items.ToList();

            MethodInfo getStores = typeof(StoresController).GetMethods()
                .First(method => method.Name == nameof(StoresController.GetAsync) && method.GetParameters().Length == 2);

            ApiDescription getStoresDescription = descriptions.First(description => (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo ==
                getStores);

            getStoresDescription.Should().NotBeNull();
            getStoresDescription.SupportedResponseTypes.Should().HaveCount(1);

            ApiResponseFormat jsonApiResponse = getStoresDescription.SupportedResponseTypes[0].ApiResponseFormats
                .FirstOrDefault(format => format.Formatter.GetType().Implements(typeof(IJsonApiOutputFormatter)));

            jsonApiResponse.Should().BeNull();
        }
    }
}
