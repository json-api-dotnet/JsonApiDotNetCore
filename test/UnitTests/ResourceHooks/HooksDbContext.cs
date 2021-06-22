using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using UnitTests.ResourceHooks.Models;

// @formatter:wrap_chained_method_calls chop_always

namespace UnitTests.ResourceHooks
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class HooksDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Article> Articles { get; set; }

        public HooksDbContext(DbContextOptions<HooksDbContext> options)
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
                .HasOne(person => person.StakeholderTodoItem)
                .WithMany(todoItem => todoItem.Stakeholders)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TodoItem>()
                .HasMany(todoItem => todoItem.ChildTodoItems)
                .WithOne(todoItem => todoItem.ParentTodo);

            builder.Entity<Passport>()
                .HasOne(passport => passport.Person)
                .WithOne(person => person.Passport)
                .HasForeignKey<Person>("PassportId")
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TodoItem>()
                .HasOne(todoItem => todoItem.OneToOnePerson)
                .WithOne(person => person.OneToOneTodoItem)
                .HasForeignKey<TodoItem>("OneToOnePersonId");
        }
    }
}
