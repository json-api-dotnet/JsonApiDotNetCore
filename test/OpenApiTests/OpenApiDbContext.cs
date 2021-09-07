using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace OpenApiTests
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OpenApiDbContext : DbContext
    {
        public DbSet<Airplane> Airplanes { get; set; }
        public DbSet<Flight> Flights { get; set; }

        public OpenApiDbContext(DbContextOptions<OpenApiDbContext> options)
            : base(options)
        {
        }
    }
}
