using GettingStarted.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class SampleDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }

        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Person>();
        }
    }
}
