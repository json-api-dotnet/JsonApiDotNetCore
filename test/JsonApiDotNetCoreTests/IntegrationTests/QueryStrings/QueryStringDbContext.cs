using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class QueryStringDbContext : DbContext
{
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<BlogPost> Posts => Set<BlogPost>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<WebAccount> Accounts => Set<WebAccount>();
    public DbSet<Human> Humans => Set<Human>();
    public DbSet<Man> Men => Set<Man>();
    public DbSet<Woman> Women => Set<Woman>();
    public DbSet<AccountPreferences> AccountPreferences => Set<AccountPreferences>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public QueryStringDbContext(DbContextOptions<QueryStringDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<WebAccount>()
            .HasMany(webAccount => webAccount.Posts)
            .WithOne(blogPost => blogPost.Author!);

        builder.Entity<Man>()
            .HasOne(man => man.Wife)
            .WithOne(woman => woman.Husband)
            .HasForeignKey<Man>();
    }
}
