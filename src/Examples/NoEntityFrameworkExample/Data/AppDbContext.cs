using NoEntityFrameworkExample.Models;
using Microsoft.EntityFrameworkCore;

namespace NoEntityFrameworkExample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>()
                .Property(t => t.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        }

        public DbSet<TodoItem> TodoItems { get; set; }
    }


}
