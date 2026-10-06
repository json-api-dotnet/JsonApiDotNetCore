using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.ClientSetNullOnDelete;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ClientSetNullOnDeleteDbContext(DbContextOptions<ClientSetNullOnDeleteDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ShoppingBasket> ShoppingBaskets => Set<ShoppingBasket>();
    public DbSet<OrderAudit> OrderAudits => Set<OrderAudit>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Customer>()
            .HasMany(customer => customer.Orders)
            .WithOne(order => order.Customer);

        builder.Entity<Customer>()
            .HasOne(customer => customer.FirstOrder)
            .WithOne()
            .HasForeignKey<Customer>("FirstOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Customer>()
            .HasOne(customer => customer.LastOrder)
            .WithOne()
            .HasForeignKey<Customer>("LastOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Order>()
            .HasOne(order => order.Parent)
            .WithMany()
            .HasForeignKey("ParentOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<ShoppingBasket>()
            .HasOne(shoppingBasket => shoppingBasket.CurrentOrder)
            .WithOne()
            .HasForeignKey<ShoppingBasket>("CurrentOrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<SupportTicket>()
            .HasOne(ticket => ticket.Order)
            .WithMany()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<OrderAudit>()
            .HasOne(audit => audit.Order)
            .WithOne()
            .HasForeignKey<OrderAudit>("OrderId")
            .OnDelete(DeleteBehavior.ClientSetNull);

        base.OnModelCreating(builder);
    }

    protected override bool ShouldOverrideDeleteBehavior(IMutableForeignKey foreignKey)
    {
        return false;
    }
}
