using JetBrains.Annotations;
using JsonApiDotNetCoreExample.Startups;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreExampleTests
{
    /// <summary>
    /// A test context for tests that reference the JsonApiDotNetCoreExample project.
    /// </summary>
    /// <typeparam name="TStartup">
    /// The server Startup class, which can be defined in the test project.
    /// </typeparam>
    /// <typeparam name="TDbContext">
    /// The EF Core database context, which can be defined in the test project.
    /// </typeparam>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ExampleIntegrationTestContext<TStartup, TDbContext> : BaseIntegrationTestContext<TStartup, EmptyStartup, TDbContext>
        where TStartup : class
        where TDbContext : DbContext
    {
    }
}
