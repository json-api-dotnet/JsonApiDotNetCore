using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
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
