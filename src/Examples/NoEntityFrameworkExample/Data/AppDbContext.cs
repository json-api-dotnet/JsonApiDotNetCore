using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AppDbContext : DbContext
    {
        public DbSet<WorkItem> WorkItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
