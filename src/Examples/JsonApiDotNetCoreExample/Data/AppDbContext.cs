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
        public DbSet<Blog> Blogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SuperUser>().HasBaseType<User>();

            modelBuilder.Entity<TodoItem>()
                .Property(t => t.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.Assignee)
                .WithMany(p => p.AssignedTodoItems);

            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.Owner)
                .WithMany(p => p.TodoItems);

            modelBuilder.Entity<ArticleTag>()
                .HasKey(bc => new {bc.ArticleId, bc.TagId});

            modelBuilder.Entity<IdentifiableArticleTag>()
                .HasKey(bc => new {bc.ArticleId, bc.TagId});

            modelBuilder.Entity<Person>()
                .HasOne(t => t.StakeHolderTodoItem)
                .WithMany(t => t.StakeHolders)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TodoItem>()
                .HasMany(t => t.ChildrenTodos)
                .WithOne(t => t.ParentTodo);

            modelBuilder.Entity<Passport>()
                .HasOne(p => p.Person)
                .WithOne(p => p.Passport)
                .HasForeignKey<Person>("PassportKey")
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TodoItem>()
                .HasOne(p => p.OneToOnePerson)
                .WithOne(p => p.OneToOneTodoItem)
                .HasForeignKey<TodoItem>("OneToOnePersonKey");

            modelBuilder.Entity<TodoItemCollection>()
                .HasOne(p => p.Owner)
                .WithMany(p => p.TodoCollections)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Role)
                .WithOne(p => p.Person)
                .HasForeignKey<Person>("PersonRoleKey");
        }
    }
}
