using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.NamingConvention;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class NamingConventionDbContext : DbContext
{
    public DbSet<Supermarket> Supermarkets => Set<Supermarket>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();

    public NamingConventionDbContext(DbContextOptions<NamingConventionDbContext> options)
        : base(options)
    {
    }
}
