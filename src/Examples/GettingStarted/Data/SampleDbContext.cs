using GettingStarted.Models;
using GettingStarted.ResourceDefinitionExample;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Data
{
    public class SampleDbContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }

        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>();
            modelBuilder.Entity<Model>();
        }
    }
}
