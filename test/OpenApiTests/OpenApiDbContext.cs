using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

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
