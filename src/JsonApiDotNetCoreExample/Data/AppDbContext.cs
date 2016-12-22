using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        { }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }
    }
}
