using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ContentNegotiation
{
    public class IntegrationTestFixture<TStartup, TDbContext> : IClassFixture<ExampleIntegrationTestContext<TStartup, TDbContext>>
        where TStartup : class
        where TDbContext : DbContext
    {
        protected TestControllerProvider TestControllerProvider = new TestControllerProvider();

        public IntegrationTestFixture()
        {
        }

        public IntegrationTestFixture(ExampleIntegrationTestContext<TStartup, TDbContext> testContext)
        {
            TestControllerProvider.NamespaceEntryPoints.Add(GetType());


            testContext.ConfigureServicesBeforeStartup(services =>
            {
                services.RemoveControllerFeatureProviders();

                services.UseControllers(TestControllerProvider);
            });
        }
    }
}
