using System;
using JetBrains.Annotations;
using JsonApiDotNetCoreExample.Cosmos.Models;
using Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable AV1706 // Identifier contains an abbreviation or is too short

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExample.Cosmos.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AppDbContext : DbContext
    {
        public DbSet<Person> People => Set<Person>();

        public DbSet<TodoItem> TodoItems => Set<TodoItem>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultContainer("PeopleAndTodoItems");
            builder.UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.HasPartitionKey(e => e.PartitionKey);
            });

            builder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.HasPartitionKey(e => e.PartitionKey);

                // @formatter:off

                entity.Property(e => e.Priority)
                    .HasConversion(
                        value => value.ToString(),
                        value => (TodoItemPriority)Enum.Parse(typeof(TodoItemPriority), value));

                // @formatter:on

                entity.HasOne(todoItem => todoItem.Owner)
                    .WithMany(person => person.OwnedTodoItems)
                    .HasForeignKey(todoItem => todoItem.OwnerId)
                    .IsRequired();

                entity.HasOne(todoItem => todoItem.Assignee)
                    .WithMany(person => person!.AssignedTodoItems)
                    .HasForeignKey(todoItem => todoItem.AssigneeId)
                    .IsRequired(false);

                entity.OwnsMany(todoItem => todoItem.Tags);
            });
        }
    }
}
