using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ErrorDbContext : DbContext
    {
        public DbSet<ConsumerArticle> ConsumerArticles { get; set; }
        public DbSet<ThrowingArticle> ThrowingArticles { get; set; }

        public ErrorDbContext(DbContextOptions<ErrorDbContext> options)
            : base(options)
        {
        }
    }
}
