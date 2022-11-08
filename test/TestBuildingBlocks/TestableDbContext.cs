using System.Diagnostics;
using JsonApiDotNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

public abstract class TestableDbContext : DbContext
{
    protected TestableDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Writes SQL statements to the Output Window when debugging.
        builder.LogTo(message => Debug.WriteLine(message), DbLoggerCategory.Database.Name.AsArray(), LogLevel.Information);
    }
}
