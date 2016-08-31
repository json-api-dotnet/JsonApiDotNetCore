using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.Data.TestData
{
  public class TestContext : DbContext
  {
    public TestContext(DbContextOptions options)
      : base(options)
    {
    }

    public virtual DbSet<TodoItem> TodoItems { get; set; }
  }
}
