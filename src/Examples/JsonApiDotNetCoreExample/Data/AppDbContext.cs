using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }

        [Resource("todo-collections")]
        public DbSet<TodoItemCollection> TodoItemCollections { get; set; }

        [Resource("camelCasedModels")]
        public DbSet<CamelCasedModel> CamelCasedModels { get; set; }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<NonJsonApiResource> NonJsonApiResources { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PersonRole> PersonRoles { get; set; }
    }
}
