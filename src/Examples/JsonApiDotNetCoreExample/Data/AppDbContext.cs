using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExample.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Passport> Passports { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<TodoItemCollection> TodoItemCollections { get; set; }
        public DbSet<CamelCasedModel> CamelCasedModels { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> AuthorDifferentDbContextName { get; set; }
        public DbSet<NonJsonApiResource> NonJsonApiResources { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SuperUser> SuperUsers { get; set; }
        public DbSet<PersonRole> PersonRoles { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }
        public DbSet<IdentifiableArticleTag> IdentifiableArticleTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SuperUser>().HasBaseType<User>();

            modelBuilder.Entity<TodoItem>()
                .Property(t => t.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.Assignee)
                .WithMany(p => p.AssignedTodoItems)
                .HasForeignKey(t => t.AssigneeId);

            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.Owner)
                .WithMany(p => p.TodoItems)
                .HasForeignKey(t => t.OwnerId);

            modelBuilder.Entity<ArticleTag>()
                .HasKey(bc => new { bc.ArticleId, bc.TagId });

            modelBuilder.Entity<IdentifiableArticleTag>()
                .HasKey(bc => new { bc.ArticleId, bc.TagId });

            modelBuilder.Entity<Person>()
                .HasOne(t => t.StakeHolderTodo)
                .WithMany(t => t.StakeHolders)
                .HasForeignKey(t => t.StakeHolderTodoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.DependentTodoItem);

            modelBuilder.Entity<TodoItem>()
                .HasMany(t => t.ChildrenTodoItems)
                .WithOne(t => t.ParentTodoItem)
                .HasForeignKey(t => t.ParentTodoItemId);

            modelBuilder.Entity<Passport>()
                .HasOne(p => p.Person)
                .WithOne(p => p.Passport)
                .HasForeignKey<Person>(p => p.PassportId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TodoItem>()
                .HasOne(p => p.ToOnePerson)
                .WithOne(p => p.ToOneTodoItem)
                .HasForeignKey<TodoItem>(p => p.ToOnePersonId);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.ToOneTodoItem)
                .WithOne(p => p.ToOnePerson)
                .HasForeignKey<TodoItem>(p => p.ToOnePersonId);
        }
    }
}
