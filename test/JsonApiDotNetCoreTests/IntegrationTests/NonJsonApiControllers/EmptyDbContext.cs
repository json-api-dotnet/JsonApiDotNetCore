using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class EmptyDbContext : TestableDbContext
{
    public EmptyDbContext(DbContextOptions<EmptyDbContext> options)
        : base(options)
    {
    }
}
