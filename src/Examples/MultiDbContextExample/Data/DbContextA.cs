using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DbContextA : DbContext
    {
        public DbSet<ResourceA> ResourceAs { get; set; }

        public DbContextA(DbContextOptions<DbContextA> options)
            : base(options)
        {
        }
    }
}
