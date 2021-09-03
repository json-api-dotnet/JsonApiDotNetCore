using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ActionResultDbContext : DbContext
    {
        public DbSet<Toothbrush> Toothbrushes { get; set; }

        public ActionResultDbContext(DbContextOptions<ActionResultDbContext> options)
            : base(options)
        {
        }
    }
}
