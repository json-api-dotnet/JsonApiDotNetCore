using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.RequiredRelationships
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DefaultBehaviorDbContext : DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Shipment> Shipments => Set<Shipment>();

        public DefaultBehaviorDbContext(DbContextOptions<DefaultBehaviorDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>()
                .HasMany(customer => customer.Orders)
                .WithOne(order => order.Customer);

            // By default, EF Core generates an identifying foreign key for a required 1-to-1 relationship.
            // This means no foreign key column is generated, instead the primary keys point to each other directly.
            // That mechanism does not make sense for JSON:API, because patching a relationship would result in
            // also changing the identity of a resource. Naming the key explicitly forces to create a foreign key column.
            builder.Entity<Order>()
                .HasOne(order => order.Shipment)
                .WithOne(shipment => shipment.Order)
                .HasForeignKey<Shipment>("OrderId");
        }
    }
}
