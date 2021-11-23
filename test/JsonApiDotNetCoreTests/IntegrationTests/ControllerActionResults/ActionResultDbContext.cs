using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ActionResultDbContext : DbContext
    {
        public DbSet<Toothbrush> Toothbrushes => Set<Toothbrush>();

        public ActionResultDbContext(DbContextOptions<ActionResultDbContext> options)
            : base(options)
        {
        }
    }
}
