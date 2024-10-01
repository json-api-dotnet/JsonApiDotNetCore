using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class DbContextB(DbContextOptions<DbContextB> options)
    : DbContext(options)
{
    public DbSet<ResourceB> ResourceBs => Set<ResourceB>();
}
