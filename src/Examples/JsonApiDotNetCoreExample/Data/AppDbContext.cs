using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExample.Data
{
    public sealed class AppDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> AuthorDifferentDbContextName { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TodoItem>()
                .HasOne(t => t.Assignee)
                .WithMany(p => p.AssignedTodoItems);

            builder.Entity<TodoItem>()
                .HasOne(t => t.Owner)
                .WithMany(p => p.TodoItems);

            builder.Entity<ArticleTag>()
                .HasKey(bc => new {bc.ArticleId, bc.TagId});

            builder.Entity<IdentifiableArticleTag>()
                .HasKey(bc => new {bc.ArticleId, bc.TagId});

            builder.Entity<Person>()
                .HasOne(t => t.StakeHolderTodoItem)
                .WithMany(t => t.StakeHolders)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TodoItem>()
                .HasMany(t => t.ChildrenTodos)
                .WithOne(t => t.ParentTodo);

            builder.Entity<Passport>()
                .HasOne(p => p.Person)
                .WithOne(p => p.Passport)
                .HasForeignKey<Person>("PassportKey")
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TodoItem>()
                .HasOne(p => p.OneToOnePerson)
                .WithOne(p => p.OneToOneTodoItem)
                .HasForeignKey<TodoItem>("OneToOnePersonKey");
        }
    }
}
