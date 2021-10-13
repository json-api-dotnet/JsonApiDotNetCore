using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DbContextB : DbContext
    {
        public DbSet<ResourceB> ResourceBs => Set<ResourceB>();

        public DbContextB(DbContextOptions<DbContextB> options)
            : base(options)
        {
        }
    }
}
