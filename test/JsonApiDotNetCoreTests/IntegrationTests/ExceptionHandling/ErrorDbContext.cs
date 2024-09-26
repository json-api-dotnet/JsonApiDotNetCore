using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ErrorDbContext(DbContextOptions<ErrorDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<ConsumerArticle> ConsumerArticles => Set<ConsumerArticle>();
    public DbSet<ThrowingArticle> ThrowingArticles => Set<ThrowingArticle>();
}
