using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    public sealed class ApiRequestFormatMedataProviderTests : IClassFixture<ExampleIntegrationTestContext<ApiExplorerStartup<StoreDbContext>, StoreDbContext>>
    {
        private readonly ExampleIntegrationTestContext<ApiExplorerStartup<StoreDbContext>, StoreDbContext> _testContext;

        public ApiRequestFormatMedataProviderTests(ExampleIntegrationTestContext<ApiExplorerStartup<StoreDbContext>, StoreDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public void Can_retrieve_content_type_set_with_ConsumesAttribute_value_in_ApiExplorer()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            IReadOnlyList<ApiDescription> descriptions = groups.Single().Items.ToList();
            MethodInfo postStoresMethod = typeof(StoresController).GetMethod(nameof(StoresController.PostAsync));
            ApiDescription postStoresDescription = descriptions.First(description =>  (description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo ==
                postStoresMethod);

            postStoresDescription.Should().NotBeNull();
            postStoresDescription.SupportedRequestFormats.Should().HaveCount(1);
            postStoresDescription.SupportedRequestFormats[0].MediaType.Should().Be(HeaderConstants.MediaType);
        }
    }
}
