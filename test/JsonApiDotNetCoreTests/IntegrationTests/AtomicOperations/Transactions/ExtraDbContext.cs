using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ExtraDbContext : DbContext
    {
        public ExtraDbContext(DbContextOptions<ExtraDbContext> options)
            : base(options)
        {
        }
    }
}
