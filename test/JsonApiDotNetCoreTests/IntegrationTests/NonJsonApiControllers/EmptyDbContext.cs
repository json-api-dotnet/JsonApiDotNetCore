using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class EmptyDbContext : DbContext
{
    public EmptyDbContext(DbContextOptions<EmptyDbContext> options)
        : base(options)
    {
    }
}
