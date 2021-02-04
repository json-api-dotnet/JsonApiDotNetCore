using Microsoft.EntityFrameworkCore;

namespace TestBuildingBlocks
{
    /// <summary>
    /// A test context that creates a new database and server instance before running tests and cleans up afterwards.
    /// You can either use this as a fixture on your tests class (init/cleanup runs once before/after all tests) or
    /// have your tests class inherit from it (init/cleanup runs once before/after each test). See
    /// <see href="https://xunit.net/docs/shared-context"/> for details on shared context usage.
    /// </summary>
    /// <typeparam name="TStartup">The server Startup class, which MUST be defined in the API project.</typeparam>
    /// <typeparam name="TDbContext">The EF Core database context, which MUST be defined in the API project.</typeparam>
    public class RemoteIntegrationTestContext<TStartup, TDbContext> : BaseIntegrationTestContext<TStartup, TStartup, TDbContext>
        where TStartup : class
        where TDbContext : DbContext
    {
    }
}
