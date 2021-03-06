using JetBrains.Annotations;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExample.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AppDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> AuthorDifferentDbContextName { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TodoItem>()
                .HasOne(todoItem => todoItem.Assignee)
                .WithMany(person => person.AssignedTodoItems);

            builder.Entity<TodoItem>()
                .HasOne(todoItem => todoItem.Owner)
                .WithMany(person => person.TodoItems);

            builder.Entity<ArticleTag>()
                .HasKey(bc => new
                {
                    bc.ArticleId,
                    bc.TagId
                });

            builder.Entity<IdentifiableArticleTag>()
                .HasKey(bc => new
                {
                    bc.ArticleId,
                    bc.TagId
                });

            builder.Entity<Person>()
                .HasOne(person => person.StakeHolderTodoItem)
                .WithMany(todoItem => todoItem.StakeHolders)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TodoItem>()
                .HasMany(todoItem => todoItem.ChildTodoItems)
                .WithOne(todoItem => todoItem.ParentTodo);

            builder.Entity<Passport>()
                .HasOne(passport => passport.Person)
                .WithOne(person => person.Passport)
                .HasForeignKey<Person>("PassportKey")
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TodoItem>()
                .HasOne(todoItem => todoItem.OneToOnePerson)
                .WithOne(person => person.OneToOneTodoItem)
                .HasForeignKey<TodoItem>("OneToOnePersonKey");
        }
    }
}
