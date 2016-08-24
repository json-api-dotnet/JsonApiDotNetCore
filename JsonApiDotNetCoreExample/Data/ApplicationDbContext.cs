using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext (DbContextOptions options)
        : base(options)
    { }

    public DbSet<TodoItem> TodoItems { get; set; }
  }
}
