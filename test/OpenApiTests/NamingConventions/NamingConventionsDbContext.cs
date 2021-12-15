using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.NamingConventions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingConventionsDbContext : DbContext
{
    public DbSet<Supermarket> Supermarkets => Set<Supermarket>();

    public NamingConventionsDbContext(DbContextOptions<NamingConventionsDbContext> options)
        : base(options)
    {
    }
}
