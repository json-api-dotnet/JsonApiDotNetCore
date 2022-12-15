using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ExtraDbContext : TestableDbContext
{
    public ExtraDbContext(DbContextOptions<ExtraDbContext> options)
        : base(options)
    {
    }
}
