using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DefaultBehaviorDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Shipment> Shipments { get; set; }

        public DefaultBehaviorDbContext(DbContextOptions<DefaultBehaviorDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>()
                .HasMany(customer => customer.Orders)
                .WithOne(order => order.Customer)
                .IsRequired();

            builder.Entity<Order>()
                .HasOne(order => order.Shipment)
                .WithOne(shipment => shipment.Order)
                .HasForeignKey<Shipment>()
                .IsRequired();
        }
    }
}
