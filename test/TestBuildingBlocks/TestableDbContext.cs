using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
        WriteSqlStatementsToOutputWindow(builder);
    }

    [Conditional("DEBUG")]
    private static void WriteSqlStatementsToOutputWindow(DbContextOptionsBuilder builder)
    {
        builder.LogTo(message => Debug.WriteLine(message), [DbLoggerCategory.Database.Name], LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        foreach (IMutableForeignKey foreignKey in builder.Model.GetEntityTypes().SelectMany(entityType => entityType.GetForeignKeys()))
        {
            if (foreignKey.DeleteBehavior == DeleteBehavior.ClientSetNull)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.SetNull;
            }
        }
    }
}
