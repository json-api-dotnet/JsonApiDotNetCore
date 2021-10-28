using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace OpenApiTests.NamingConvention
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
