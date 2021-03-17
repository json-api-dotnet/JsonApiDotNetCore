using Microsoft.EntityFrameworkCore;
using Npgsql.TypeHandlers.FullTextSearchHandlers;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests
{
    public abstract class IntegrationTestFixture<TStartup, TDbContext> : IClassFixture<ExampleIntegrationTestContext<TStartup, TDbContext>>
        where TStartup : class
        where TDbContext : DbContext
    {
        protected IntegrationTestFixture(ExampleIntegrationTestContext<TStartup, TDbContext> testContext)
        {
            testContext.AddControllersInNamespaceOf<TDbContext>();
        }
    }
}
