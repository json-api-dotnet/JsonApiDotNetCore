using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class WorkflowDbContext : TestableDbContext
{
    public DbSet<Workflow> Workflows => Set<Workflow>();

    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
        : base(options)
    {
    }
}
