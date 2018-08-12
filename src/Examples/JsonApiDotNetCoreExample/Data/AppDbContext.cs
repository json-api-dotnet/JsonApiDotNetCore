using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCoreExample.Models.Entities;

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

            modelBuilder.Entity<CourseStudentEntity>()
                .HasKey(r => new { r.CourseId, r.StudentId });

            modelBuilder.Entity<CourseStudentEntity>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(r => r.CourseId)
                ;

            modelBuilder.Entity<CourseStudentEntity>()
                .HasOne(r => r.Student)
                .WithMany(s => s.Courses)
                .HasForeignKey(r => r.StudentId);
        }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<TodoItemCollection> TodoItemCollections { get; set; }
        public DbSet<CamelCasedModel> CamelCasedModels { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<NonJsonApiResource> NonJsonApiResources { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<DepartmentEntity> Departments { get; set; }
        public DbSet<CourseStudentEntity> Registrations { get; set; }
        public DbSet<StudentEntity> Students { get; set; }
        
        public DbSet<PersonRole> PersonRoles { get; set; }
    }
}
