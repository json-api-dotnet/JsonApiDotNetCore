using DapperExample.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// @formatter:wrap_chained_method_calls chop_always

namespace DapperExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<LoginAccount> LoginAccounts => Set<LoginAccount>();
    public DbSet<AccountRecovery> AccountRecoveries => Set<AccountRecovery>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RgbColor> RgbColors => Set<RgbColor>();

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Person>()
            .HasMany(person => person.AssignedTodoItems)
            .WithOne(todoItem => todoItem.Assignee);

        builder.Entity<Person>()
            .HasMany(person => person.OwnedTodoItems)
            .WithOne(todoItem => todoItem.Owner);

        builder.Entity<Person>()
            .HasOne(person => person.Account)
            .WithOne(loginAccount => loginAccount.Person)
            .HasForeignKey<Person>("AccountId");

        builder.Entity<LoginAccount>()
            .HasOne(loginAccount => loginAccount.Recovery)
            .WithOne(accountRecovery => accountRecovery.Account)
            .HasForeignKey<LoginAccount>("RecoveryId");

        builder.Entity<Tag>()
            .HasOne(tag => tag.Color)
            .WithOne(rgbColor => rgbColor.Tag)
            .HasForeignKey<RgbColor>("TagId");

        var databaseProvider = _configuration.GetValue<DatabaseProvider>("DatabaseProvider");

        if (databaseProvider != DatabaseProvider.SqlServer)
        {
            // In this example project, all cascades happen in the database, but SQL Server doesn't support that very well.
            AdjustDeleteBehaviorForJsonApi(builder);
        }
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
