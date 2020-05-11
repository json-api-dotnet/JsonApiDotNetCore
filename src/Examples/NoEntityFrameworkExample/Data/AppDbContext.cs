using Microsoft.EntityFrameworkCore;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Data
{
    public sealed class AppDbContext : DbContext
    {
        public DbSet<WorkItem> WorkItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
