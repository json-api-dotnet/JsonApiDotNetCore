using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ConcurrencyDbContext : DbContext
{
    public DbSet<WebPage> WebPages => Set<WebPage>();
    public DbSet<FriendlyUrl> FriendlyUrls => Set<FriendlyUrl>();
    public DbSet<TextBlock> TextBlocks => Set<TextBlock>();
    public DbSet<Paragraph> Paragraphs => Set<Paragraph>();
    public DbSet<WebImage> WebImages => Set<WebImage>();
    public DbSet<PageFooter> PageFooters => Set<PageFooter>();
    public DbSet<WebLink> WebLinks => Set<WebLink>();
    public DbSet<DeploymentJob> DeploymentJobs => Set<DeploymentJob>();

    public ConcurrencyDbContext(DbContextOptions<ConcurrencyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<WebPage>()
            .HasOne(webPage => webPage.Url)
            .WithOne(friendlyUrl => friendlyUrl.Page!)
            .HasForeignKey<WebPage>("FriendlyUrlId");
    }
}
