using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
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
