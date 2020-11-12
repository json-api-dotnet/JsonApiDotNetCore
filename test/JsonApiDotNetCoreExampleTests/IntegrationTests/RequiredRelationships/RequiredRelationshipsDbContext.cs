using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RequiredRelationships
{
    public sealed class RequiredRelationshipsDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Order> Deliveries { get; set; }

        public RequiredRelationshipsDbContext(DbContextOptions<RequiredRelationshipsDbContext> options)
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
                .HasOne(order => order.Delivery)
                .WithOne()
                .HasForeignKey<Delivery>()
                .IsRequired();
        }
    }
}
