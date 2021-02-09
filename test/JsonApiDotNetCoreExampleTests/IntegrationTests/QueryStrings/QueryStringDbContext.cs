using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class QueryStringDbContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> Posts { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<WebAccount> Accounts { get; set; }
        public DbSet<AccountPreferences> AccountPreferences { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public QueryStringDbContext(DbContextOptions<QueryStringDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BlogPostLabel>()
                .HasKey(blogPostLabel => new {blogPostLabel.BlogPostId, blogPostLabel.LabelId});

            builder.Entity<WebAccount>()
                .HasMany(webAccount => webAccount.Posts)
                .WithOne(blogPost => blogPost.Author);
        }
    }
}
