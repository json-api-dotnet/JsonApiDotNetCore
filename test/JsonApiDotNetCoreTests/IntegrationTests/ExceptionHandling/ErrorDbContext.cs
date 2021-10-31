using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ErrorDbContext : DbContext
    {
        public DbSet<ConsumerArticle> ConsumerArticles => Set<ConsumerArticle>();
        public DbSet<ThrowingArticle> ThrowingArticles => Set<ThrowingArticle>();

        public ErrorDbContext(DbContextOptions<ErrorDbContext> options)
            : base(options)
        {
        }
    }
}
