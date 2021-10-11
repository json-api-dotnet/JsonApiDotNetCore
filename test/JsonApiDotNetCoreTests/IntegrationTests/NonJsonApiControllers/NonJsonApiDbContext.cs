#nullable disable

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class NonJsonApiDbContext : DbContext
    {
        public NonJsonApiDbContext(DbContextOptions<NonJsonApiDbContext> options)
            : base(options)
        {
        }
    }
}
