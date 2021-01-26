using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class QueryStringDbContext : DbContext
    {
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public QueryStringDbContext(DbContextOptions<QueryStringDbContext> options) : base(options)
        {
        }
    }
}
