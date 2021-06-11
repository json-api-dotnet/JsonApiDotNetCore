using JetBrains.Annotations;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class HooksDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> AuthorDifferentDbContextName { get; set; }
        public DbSet<User> Users { get; set; }

        public HooksDbContext(DbContextOptions<HooksDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ArticleTag>()
                .HasKey(bc => new
                {
                    bc.ArticleId,
                    bc.TagId
                });

            builder.Entity<Person>()
                .HasOne(person => person.StakeholderTodoItem)
                .WithMany(todoItem => todoItem.Stakeholders)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Passport>()
                .HasOne(passport => passport.Person)
                .WithOne(person => person.Passport)
                .HasForeignKey<Person>("PassportKey")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
