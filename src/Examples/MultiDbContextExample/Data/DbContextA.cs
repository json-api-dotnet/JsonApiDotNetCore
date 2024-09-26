using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class DbContextA(DbContextOptions<DbContextA> options)
    : DbContext(options)
{
    public DbSet<ResourceA> ResourceAs => Set<ResourceA>();
}
