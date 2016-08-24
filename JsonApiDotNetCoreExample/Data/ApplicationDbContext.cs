using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExample.Data
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext (DbContextOptions options)
        : base(options)
    { }

    public DbSet<TodoItem> TodoItems { get; set; }
  }
}
