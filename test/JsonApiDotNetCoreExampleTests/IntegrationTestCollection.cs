using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JsonApiDotNetCoreExampleTests
{
    public abstract class IntegrationTestCollection<TStartup, TDbContext> : IClassFixture<ExampleIntegrationTestContext<TStartup, TDbContext>>
        where TStartup : class
        where TDbContext : DbContext
    {
        protected IntegrationTestCollection(ExampleIntegrationTestContext<TStartup, TDbContext> testContext)
        {
            testContext.AddControllersInNamespaceOf<TDbContext>();
        }
    }
}
