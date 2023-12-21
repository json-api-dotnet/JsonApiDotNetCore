using JetBrains.Annotations;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

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
