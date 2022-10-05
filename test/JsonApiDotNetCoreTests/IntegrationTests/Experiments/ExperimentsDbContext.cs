using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.Experiments;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ExperimentsDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ShoppingBasket> ShoppingBaskets => Set<ShoppingBasket>();

    public ExperimentsDbContext(DbContextOptions<ExperimentsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // https://stackoverflow.com/questions/54326165/ef-core-why-clientsetnull-is-default-ondelete-behavior-for-optional-relations
        // https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete

        builder.Entity<Customer>()
            .HasMany(customer => customer.Orders)
            .WithOne(order => order.Customer);

        builder.Entity<Customer>()
            .HasOne(customer => customer.FirstOrder)
            .WithOne()
            .HasForeignKey<Customer>("FirstOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);
        //.OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Customer>()
            .HasOne(customer => customer.LastOrder)
            .WithOne()
            .HasForeignKey<Customer>("LastOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);
        //.OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasOne(order => order.Parent)
            .WithOne()
            .HasForeignKey<Order>("ParentOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);
        //.OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ShoppingBasket>()
            .HasOne(shoppingBasket => shoppingBasket.CurrentOrder)
            .WithOne()
            .HasForeignKey<ShoppingBasket>("CurrentOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);
        //.OnDelete(DeleteBehavior.SetNull);
    }
}
