using GettingStarted.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GettingStarted.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class PostgreSqlSampleDbContext(DbContextOptions<PostgreSqlSampleDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
}
