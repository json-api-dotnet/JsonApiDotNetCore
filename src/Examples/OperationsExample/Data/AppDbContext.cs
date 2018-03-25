using Microsoft.EntityFrameworkCore;
using OperationsExample.Models;

namespace OperationsExample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        { }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> Authors { get; set; }
    }
}
