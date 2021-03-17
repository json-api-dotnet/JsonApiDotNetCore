using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace TestBuildingBlocks
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class NoModelsDbContext : DbContext
    {
        public NoModelsDbContext(DbContextOptions<NoModelsDbContext> options)
            : base(options)
        {
        }
    }
}
