using GettingStarted.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class SampleDbContext : DbContext
    {
        public DbSet<Book> Books => Set<Book>();

        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }
    }
}
