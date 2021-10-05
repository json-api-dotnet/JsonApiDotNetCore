using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class QueryStringDbContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> Posts { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<WebAccount> Accounts { get; set; }
        public DbSet<AccountPreferences> AccountPreferences { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public QueryStringDbContext(DbContextOptions<QueryStringDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<WebAccount>()
                .HasMany(webAccount => webAccount.Posts)
                .WithOne(blogPost => blogPost.Author);
        }
    }
}
