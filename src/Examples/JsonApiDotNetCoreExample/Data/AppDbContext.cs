using JetBrains.Annotations;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Tag = JsonApiDotNetCoreExample.Models.Tag;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TagTodoItem> TagTodoItems => Set<TagTodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // When deleting a person, un-assign him/her from existing todo-items.
        builder.Entity<Person>()
            .HasMany(person => person.AssignedTodoItems)
            .WithOne(todoItem => todoItem.Assignee);

        // When deleting a person, the todo-items he/she owns are deleted too.
        builder.Entity<Person>()
            .HasMany(person => person.OwnedTodoItems)
            .WithOne(todoItem => todoItem.Owner);

        // Use an explicit join entity for the many-to-many between Tag and TodoItem so EF Core
        // does not create a shadow join table represented as Dictionary<string, object>.
        builder.Entity<Tag>()
            .HasMany<TodoItem>(tag => tag.TodoItems)
            .WithMany(todoItem => todoItem.Tags)
            .UsingEntity<TagTodoItem>(rightSide => rightSide
                    .HasOne(tagTodoItem => tagTodoItem.TodoItem)
                    .WithMany(todoItem => todoItem.TagTodoItems)
                    .HasForeignKey(tagTodoItem => tagTodoItem.TodoItemId),
                leftSide => leftSide
                    .HasOne(tagTodoItem => tagTodoItem.Tag)
                    .WithMany(tag => tag.TagTodoItems)
                    .HasForeignKey(tagTodoItem => tagTodoItem.TagId),
                joinEntity =>
                {
                    joinEntity.HasKey(tagTodoItem => new
                    {
                        tagTodoItem.TagId,
                        tagTodoItem.TodoItemId
                    });
                });

        AdjustDeleteBehaviorForJsonApi(builder);
    }

    private static void AdjustDeleteBehaviorForJsonApi(ModelBuilder builder)
    {
        foreach (IMutableForeignKey foreignKey in builder.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetForeignKeys()))
        {
            if (foreignKey.DeleteBehavior == DeleteBehavior.ClientSetNull)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.SetNull;
            }

            if (foreignKey.DeleteBehavior == DeleteBehavior.ClientCascade)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
            }
        }
    }
}
