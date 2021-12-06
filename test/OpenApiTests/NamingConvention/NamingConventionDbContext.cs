using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.NamingConventions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class NamingConventionDbContext : DbContext
    {
        public DbSet<Supermarket> Supermarkets { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }

        public NamingConventionDbContext(DbContextOptions<NamingConventionDbContext> options)
            : base(options)
        {
        }
    }
}
