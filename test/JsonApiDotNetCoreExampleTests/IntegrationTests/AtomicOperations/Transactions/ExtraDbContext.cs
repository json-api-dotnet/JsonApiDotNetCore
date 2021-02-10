using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Transactions
{
    public sealed class ExtraDbContext : DbContext
    {
        public ExtraDbContext(DbContextOptions<ExtraDbContext> options)
            : base(options)
        {
        }
    }
}
