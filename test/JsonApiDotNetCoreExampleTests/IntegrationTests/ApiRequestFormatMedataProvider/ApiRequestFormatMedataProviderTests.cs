using System.Collections.Generic;
using System.Linq;
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

        // TODO: Clean up, this is a draft.
        [Fact]
        public async Task Input_formatters()
        {
            // Arrange
            var provider = _testContext.Factory.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            // Act
            IReadOnlyList<ApiDescriptionGroup> groups = provider.ApiDescriptionGroups.Items;

            // Assert
            List<ApiDescription> descriptions = groups.Single().Items.Where(description => description.SupportedRequestFormats.Count == 1).ToList();

            ApiDescription operationsDescription = descriptions.First(descriptor =>
                ((ControllerActionDescriptor)descriptor.ActionDescriptor).ControllerTypeInfo == typeof(OperationsController));

            operationsDescription.SupportedRequestFormats.Should().HaveCount(1);

            operationsDescription.SupportedRequestFormats[0].MediaType.Should().Be(HeaderConstants.AtomicOperationsMediaType.Replace(";", "; "));

            descriptions.Remove(operationsDescription);

            descriptions.Should().HaveCount(1);
            descriptions[0].SupportedRequestFormats.Should().HaveCount(1);
            descriptions[0].SupportedRequestFormats[0].MediaType.Should().Be(HeaderConstants.MediaType);
        }
    }
}
