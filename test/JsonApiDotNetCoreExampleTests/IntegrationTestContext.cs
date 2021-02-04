using JsonApiDotNetCoreExample;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests
{
    /// <summary>
    /// A test context that creates a new database and server instance before running tests and cleans up afterwards.
    /// You can either use this as a fixture on your tests class (init/cleanup runs once before/after all tests) or
    /// have your tests class inherit from it (init/cleanup runs once before/after each test). See
    /// <see href="https://xunit.net/docs/shared-context"/> for details on shared context usage.
    /// </summary>
    /// <typeparam name="TStartup">The server Startup class, which can be defined in the test project.</typeparam>
    /// <typeparam name="TDbContext">The EF Core database context, which can be defined in the test project.</typeparam>
    public class IntegrationTestContext<TStartup, TDbContext> : BaseIntegrationTestContext<TStartup, EmptyStartup, TDbContext>
        where TStartup : class
        where TDbContext : DbContext
    {
    }
}
